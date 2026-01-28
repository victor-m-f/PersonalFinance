using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.InvoiceImport.Settings;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class LlmJsonInterpreter
{
    private readonly HttpClient _httpClient;
    private readonly ILlmSettingsProvider _settingsProvider;
    private readonly LocalLlmRuntime _localRuntime;
    private readonly ILogger<LlmJsonInterpreter> _logger;

    public LlmJsonInterpreter(
        HttpClient httpClient,
        ILlmSettingsProvider settingsProvider,
        LocalLlmRuntime localRuntime,
        ILogger<LlmJsonInterpreter> logger)
    {
        _httpClient = httpClient;
        _settingsProvider = settingsProvider;
        _localRuntime = localRuntime;
        _logger = logger;
    }

    public async Task<Result<string>> GenerateJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken ct)
    {
        var options = _settingsProvider.GetOptions();
        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            return Result<string>.Failure("NotConfigured", "LLM provider not configured.");
        }

        var provider = options.Provider.Trim().ToLowerInvariant();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var timeout = Math.Max(5, options.TimeoutSeconds);
        cts.CancelAfter(TimeSpan.FromSeconds(timeout));
        try
        {
            var result = provider switch
            {
                "local" => await _localRuntime.GenerateAsync(systemPrompt, userPrompt, cts.Token),
                "openai" => await CallOpenAiAsync(options, systemPrompt, userPrompt, cts.Token),
                "azureopenai" => await CallAzureOpenAiAsync(options, systemPrompt, userPrompt, cts.Token),
                "ollama" => await CallOllamaAsync(options, systemPrompt, userPrompt, cts.Token),
                _ => Result<string>.Failure("NotSupported", "Unsupported LLM provider.")
            };
            if (!result.IsSuccess)
            {
                return result;
            }

            var normalized = NormalizeJson(result.Value!);
            return Result<string>.Success(normalized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM request failed.");
            return Result<string>.Failure("LlmFailed", "LLM request failed.");
        }
    }

    private async Task<Result<string>> CallOpenAiAsync(
        LlmProviderOptions options,
        string systemPrompt,
        string userPrompt,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return Result<string>.Failure("NotConfigured", "OpenAI endpoint or api key missing.");
        }

        var request = new
        {
            model = options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, options.Endpoint);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        var content = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Failure("LlmFailed", content);
        }

        return ExtractOpenAiContent(content);
    }

    private async Task<Result<string>> CallAzureOpenAiAsync(
        LlmProviderOptions options,
        string systemPrompt,
        string userPrompt,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.ApiKey) || string.IsNullOrWhiteSpace(options.Deployment))
        {
            return Result<string>.Failure("NotConfigured", "Azure OpenAI configuration missing.");
        }

        var endpoint = options.Endpoint.TrimEnd('/');
        var url = $"{endpoint}/openai/deployments/{options.Deployment}/chat/completions?api-version=2024-06-01";

        var request = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Add("api-key", options.ApiKey);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        var content = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Failure("LlmFailed", content);
        }

        return ExtractOpenAiContent(content);
    }

    private async Task<Result<string>> CallOllamaAsync(
        LlmProviderOptions options,
        string systemPrompt,
        string userPrompt,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.Model))
        {
            return Result<string>.Failure("NotConfigured", "Ollama endpoint or model missing.");
        }

        var request = new
        {
            model = options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            stream = false
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, options.Endpoint);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        var content = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Failure("LlmFailed", content);
        }

        return ExtractOllamaContent(content);
    }

    private static Result<string> ExtractOpenAiContent(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array)
        {
            return Result<string>.Failure("LlmFailed", "Invalid response.");
        }

        var first = choices.EnumerateArray().FirstOrDefault();
        if (first.ValueKind == JsonValueKind.Undefined)
        {
            return Result<string>.Failure("LlmFailed", "Empty response.");
        }

        if (!first.TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var content))
        {
            return Result<string>.Failure("LlmFailed", "Invalid response.");
        }

        var value = content.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<string>.Failure("LlmFailed", "Empty response.");
        }

        return Result<string>.Success(value);
    }

    private static Result<string> ExtractOllamaContent(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var content))
        {
            return Result<string>.Failure("LlmFailed", "Invalid response.");
        }

        var value = content.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<string>.Failure("LlmFailed", "Empty response.");
        }

        return Result<string>.Success(value);
    }

    private string NormalizeJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        var trimmed = raw.Trim();
        if (TryExtractJsonValue(trimmed, out var extracted))
        {
            return FixJsonCommonIssues(extracted);
        }

        _logger.LogWarning("LLM returned non-JSON content. Preview: {Preview}", trimmed.Length > 400 ? trimmed[..400] : trimmed);
        return FixJsonCommonIssues(trimmed);
    }

    private bool TryExtractJsonValue(string raw, out string json)
    {
        json = string.Empty;

        var codeBlockStart = raw.IndexOf("```", StringComparison.Ordinal);
        if (codeBlockStart >= 0)
        {
            var codeBlockEnd = raw.IndexOf("```", codeBlockStart + 3, StringComparison.Ordinal);
            if (codeBlockEnd > codeBlockStart)
            {
                var inner = raw.Substring(codeBlockStart + 3, codeBlockEnd - (codeBlockStart + 3));
                var cleaned = inner.Trim();
                if (cleaned.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                {
                    cleaned = cleaned.Substring(4).Trim();
                }

                if (TryExtractJsonValue(cleaned, out json))
                {
                    return true;
                }
            }
        }

        var startIndex = raw.IndexOf('{');
        var arrayIndex = raw.IndexOf('[');
        if (startIndex < 0 || (arrayIndex >= 0 && arrayIndex < startIndex))
        {
            startIndex = arrayIndex;
        }

        if (startIndex < 0)
        {
            return false;
        }

        var slice = raw.Substring(startIndex);
        if (TryExtractBalancedJson(slice, out json))
        {
            return true;
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(slice);
        var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        try
        {
            while (reader.Read())
            {
                if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                {
                    using var document = JsonDocument.ParseValue(ref reader);
                    json = document.RootElement.GetRawText();
                    return true;
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    private static bool TryExtractBalancedJson(string text, out string json)
    {
        json = string.Empty;
        var startIndex = text.IndexOf('{');
        var arrayIndex = text.IndexOf('[');
        if (startIndex < 0 || (arrayIndex >= 0 && arrayIndex < startIndex))
        {
            startIndex = arrayIndex;
        }

        if (startIndex < 0)
        {
            return false;
        }

        var startChar = text[startIndex];
        var endChar = startChar == '{' ? '}' : ']';
        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = startIndex; i < text.Length; i++)
        {
            var ch = text[i];
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

            if (ch == startChar)
            {
                depth++;
            }
            else if (ch == endChar)
            {
                depth--;
                if (depth == 0)
                {
                    json = text.Substring(startIndex, i - startIndex + 1);
                    return true;
                }
            }
        }

        return false;
    }

    private static string FixJsonCommonIssues(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        var fixedJson = json;

        // Fix numeric expressions like: "quantity": 141.9 / 70.95
        fixedJson = System.Text.RegularExpressions.Regex.Replace(
            fixedJson,
            @":\s*([0-9]+(?:\.[0-9]+)?)\s*/\s*([0-9]+(?:\.[0-9]+)?)",
            match =>
            {
                if (!decimal.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var left))
                {
                    return match.Value;
                }

                if (!decimal.TryParse(match.Groups[2].Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var right))
                {
                    return match.Value;
                }

                if (right == 0)
                {
                    return match.Value;
                }

                var value = left / right;
                return $": {value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)}";
            });

        // Remove leading zeros in integer values like: "itemId": 001
        fixedJson = System.Text.RegularExpressions.Regex.Replace(
            fixedJson,
            @":\s*0+(\d+)(?=[,\}\]\s])",
            match => $": {match.Groups[1].Value}");

        return fixedJson;
    }
}
