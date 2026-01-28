using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Application.InvoiceImport.Services;
using PersonalFinance.Application.InvoiceImport.Settings;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class InvoiceInterpreter : IInvoiceInterpreter
{
    private static readonly Regex DateRegex = new(@"(\d{1,2})/(\d{1,2})/(\d{2,4})", RegexOptions.Compiled);
    private static readonly Regex AmountRegex = new(@"(\d{1,3}(?:[\.\s]\d{3})*,\d{2})|(\d+\.\d{2})", RegexOptions.Compiled);

    private readonly InvoiceInterpreterOptions _options;
    private readonly LlmJsonInterpreter _llm;
    private readonly ILogger<InvoiceInterpreter> _logger;

    public InvoiceInterpreter(
        IOptions<InvoiceInterpreterOptions> options,
        LlmJsonInterpreter llm,
        ILogger<InvoiceInterpreter> logger)
    {
        _options = options.Value;
        _llm = llm;
        _logger = logger;
    }

    public async Task<Result<InvoiceInterpretationResult>> InterpretAsync(
        InterpretInvoiceRequest request,
        CancellationToken ct)
    {
        var heuristic = BuildHeuristic(request.RawText);
        var heuristicJson = JsonSerializer.Serialize(heuristic);

        if (!_options.EnableLlm)
        {
            return Result<InvoiceInterpretationResult>.Failure("LlmRequired", "LLM is required for invoice interpretation.");
        }

        var prompt = BuildUserPrompt(request.RawText);
        var llmResult = await _llm.GenerateJsonAsync(_options.SystemPrompt, prompt, ct);
        if (!llmResult.IsSuccess)
        {
            _logger.LogWarning("LLM invoice interpretation failed: {Message}", llmResult.ErrorMessage);
            return Result<InvoiceInterpretationResult>.Failure(llmResult.ErrorCode ?? "LlmFailed", llmResult.ErrorMessage ?? "LLM failed.");
        }

        var parsed = InvoiceInterpretationParser.Parse(llmResult.Value!);
        if (!parsed.IsSuccess)
        {
            _logger.LogWarning("LLM invoice JSON invalid: {Message}", parsed.ErrorMessage);
            _logger.LogWarning("LLM invoice JSON preview: {Preview}", llmResult.Value!.Length > 400 ? llmResult.Value![..400] : llmResult.Value!);
            var simplified = RemoveLineItems(llmResult.Value!);
            if (!string.Equals(simplified, llmResult.Value!, StringComparison.Ordinal))
            {
                var retry = InvoiceInterpretationParser.Parse(simplified);
                if (retry.IsSuccess)
                {
                    return Result<InvoiceInterpretationResult>.Success(new InvoiceInterpretationResult
                    {
                        Json = simplified,
                        Data = retry.Value!
                    });
                }
            }

            return Result<InvoiceInterpretationResult>.Failure(parsed.ErrorCode ?? "ValidationError", parsed.ErrorMessage ?? "Invalid JSON.");
        }

        return Result<InvoiceInterpretationResult>.Success(new InvoiceInterpretationResult
        {
            Json = llmResult.Value!,
            Data = parsed.Value!
        });
    }

    private InvoiceInterpretationResponse BuildHeuristic(string rawText)
    {
        var vendor = ExtractVendor(rawText);
        var date = ExtractDate(rawText) ?? DateTime.Today;
        var total = ExtractTotal(rawText);
        var currency = ExtractCurrency(rawText);

        return new InvoiceInterpretationResponse
        {
            VendorName = vendor,
            InvoiceDate = date,
            TotalAmount = total,
            Currency = currency,
            Notes = "Heuristic",
            Confidence = 0.4d,
            LineItems = null
        };
    }

    private string BuildUserPrompt(string rawText)
    {
        var template = string.IsNullOrWhiteSpace(_options.UserPromptTemplate)
            ? "Extract invoice data from the text and respond only with JSON. Text: {{text}}"
            : _options.UserPromptTemplate;

        return template.Replace("{{text}}", rawText ?? string.Empty);
    }

    private static string ExtractVendor(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return "Unknown";
        }

        var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var candidate = lines.FirstOrDefault(line => line.Length >= 3) ?? "Unknown";
        return candidate.Trim();
    }

    private static DateTime? ExtractDate(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return null;
        }

        var match = DateRegex.Matches(rawText).Cast<Match>().FirstOrDefault(x => x.Success);
        if (match is null)
        {
            return null;
        }

        var day = ParseInt(match.Groups[1].Value);
        var month = ParseInt(match.Groups[2].Value);
        var year = ParseInt(match.Groups[3].Value);
        if (year < 100)
        {
            year += 2000;
        }

        if (DateTime.TryParseExact(
                $"{day:00}/{month:00}/{year:0000}",
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return date;
        }

        return null;
    }

    private static decimal ExtractTotal(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return 0m;
        }

        var amounts = new List<decimal>();
        foreach (Match match in AmountRegex.Matches(rawText))
        {
            if (!match.Success)
            {
                continue;
            }

            if (TryParseAmount(match.Value, out var amount))
            {
                amounts.Add(amount);
            }
        }

        return amounts.Count == 0 ? 0m : amounts.Max();
    }

    private static bool TryParseAmount(string value, out decimal amount)
    {
        var normalized = value.Trim();
        if (normalized.Contains(','))
        {
            normalized = normalized.Replace(".", string.Empty);
            normalized = normalized.Replace(',', '.');
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    private static string ExtractCurrency(string rawText)
    {
        if (rawText.Contains("R$", StringComparison.OrdinalIgnoreCase))
        {
            return "BRL";
        }

        if (rawText.Contains("$", StringComparison.OrdinalIgnoreCase))
        {
            return "USD";
        }

        if (rawText.Contains("€", StringComparison.OrdinalIgnoreCase))
        {
            return "EUR";
        }

        if (rawText.Contains("£", StringComparison.OrdinalIgnoreCase))
        {
            return "GBP";
        }

        return "BRL";
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, out var result) ? result : 0;
    }

    private static string RemoveLineItems(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        var pattern = @"""lineItems""\s*:\s*\[";
        var start = System.Text.RegularExpressions.Regex.Match(json, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!start.Success)
        {
            return json;
        }

        var index = start.Index;
        var slice = json.Substring(index);
        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = 0; i < slice.Length; i++)
        {
            var ch = slice[i];
            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == '[')
            {
                depth++;
            }
            else if (ch == ']')
            {
                depth--;
                if (depth == 0)
                {
                    var endIndex = index + i + 1;
                    var before = json.Substring(0, index);
                    var after = json.Substring(endIndex);
                    return System.Text.RegularExpressions.Regex.Replace(before + "\"lineItems\": null" + after, @",\s*([}\]])", "$1");
                }
            }
        }

        return json;
    }
}
