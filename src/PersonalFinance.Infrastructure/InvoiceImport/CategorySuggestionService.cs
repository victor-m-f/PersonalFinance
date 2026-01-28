using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Application.InvoiceImport.Settings;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Search;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class CategorySuggestionService : ICategorySuggestionService
{
    private readonly ICategorySuggestionCache _cache;
    private readonly IVendorCategoryRuleRepository _rulesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CategorySuggestionOptions _options;
    private readonly LlmJsonInterpreter _llm;
    private readonly ILogger<CategorySuggestionService> _logger;

    public CategorySuggestionService(
        ICategorySuggestionCache cache,
        IVendorCategoryRuleRepository rulesRepository,
        IUnitOfWork unitOfWork,
        IOptions<CategorySuggestionOptions> options,
        LlmJsonInterpreter llm,
        ILogger<CategorySuggestionService> logger)
    {
        _cache = cache;
        _rulesRepository = rulesRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _llm = llm;
        _logger = logger;
    }

    public async Task<Result<CategorySuggestionResponse>> SuggestAsync(CategorySuggestionRequest request, CancellationToken ct)
    {
        var ruleMatch = await SuggestFromRulesAsync(request, ct);
        if (ruleMatch is not null)
        {
            return Result<CategorySuggestionResponse>.Success(ruleMatch);
        }

        if (!_options.EnableLlm)
        {
            return Result<CategorySuggestionResponse>.Failure("LlmRequired", "LLM is required for category suggestion.");
        }

        var categories = await _cache.GetCategoriesAsync(ct);
        var llmResult = await TryLlmAsync(request, categories, ct);
        if (llmResult.IsSuccess)
        {
            return Result<CategorySuggestionResponse>.Success(llmResult.Value!);
        }

        _logger.LogWarning("LLM category suggestion failed: {Message}", llmResult.ErrorMessage);
        return Result<CategorySuggestionResponse>.Failure(llmResult.ErrorCode ?? "LlmFailed", llmResult.ErrorMessage ?? "LLM failed.");
    }

    private async Task<CategorySuggestionResponse?> SuggestFromRulesAsync(
        CategorySuggestionRequest request,
        CancellationToken ct)
    {
        var rules = await _rulesRepository.GetAllAsync(ct);
        if (rules.Count == 0)
        {
            return null;
        }

        var normalizedVendor = TextSearchNormalizer.Normalize(request.VendorName) ?? string.Empty;
        var normalizedText = TextSearchNormalizer.Normalize(request.RawText) ?? string.Empty;

        VendorCategoryRule? best = null;
        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.KeywordNormalized))
            {
                continue;
            }

            if (normalizedVendor.Contains(rule.KeywordNormalized, StringComparison.OrdinalIgnoreCase)
                || normalizedText.Contains(rule.KeywordNormalized, StringComparison.OrdinalIgnoreCase))
            {
                if (best is null || rule.Confidence.Value > best.Confidence.Value)
                {
                    best = rule;
                }
            }
        }

        if (best is null)
        {
            return null;
        }

        best.MarkUsed();
        await _rulesRepository.UpdateAsync(best, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CategorySuggestionResponse
        {
            SuggestedCategoryId = best.CategoryId,
            Confidence = best.Confidence.Value,
            Rationale = "Vendor rule"
        };
    }

    private CategorySuggestionResponse SuggestFromHeuristics(
        CategorySuggestionRequest request,
        IReadOnlyList<CategorySuggestionCategoryResponse> categories)
    {
        var normalizedVendor = TextSearchNormalizer.Normalize(request.VendorName) ?? string.Empty;
        var normalizedText = TextSearchNormalizer.Normalize(request.RawText) ?? string.Empty;
        var normalizedItems = request.LineItems
            .Select(item => TextSearchNormalizer.Normalize(item) ?? string.Empty)
            .ToList();

        CategorySuggestionCategoryResponse? best = null;
        var bestScore = 0d;
        var reason = "";

        foreach (var category in categories)
        {
            var normalizedName = category.NormalizedName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                continue;
            }

            if (normalizedVendor.Contains(normalizedName, StringComparison.OrdinalIgnoreCase))
            {
                if (bestScore < 0.85d)
                {
                    bestScore = 0.85d;
                    best = category;
                    reason = "Vendor match";
                }
            }
            else if (normalizedText.Contains(normalizedName, StringComparison.OrdinalIgnoreCase))
            {
                if (bestScore < 0.65d)
                {
                    bestScore = 0.65d;
                    best = category;
                    reason = "Text match";
                }
            }
            else if (normalizedItems.Any(item => item.Contains(normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                if (bestScore < 0.7d)
                {
                    bestScore = 0.7d;
                    best = category;
                    reason = "Line item match";
                }
            }
        }

        return new CategorySuggestionResponse
        {
            SuggestedCategoryId = best?.Id,
            Confidence = bestScore,
            Rationale = string.IsNullOrWhiteSpace(reason) ? "No heuristic match" : reason
        };
    }

    private async Task<Result<CategorySuggestionResponse>> TryLlmAsync(
        CategorySuggestionRequest request,
        IReadOnlyList<CategorySuggestionCategoryResponse> categories,
        CancellationToken ct)
    {
        if (categories.Count == 0)
        {
            return Result<CategorySuggestionResponse>.Failure("NoCategories", "No categories available.");
        }

        var payload = new
        {
            vendorName = request.VendorName,
            rawText = request.RawText,
            lineItems = request.LineItems,
            categories = categories.Select(x => new { id = x.Id, name = x.Name })
        };

        var prompt = BuildPrompt(payload);
        var llmResult = await _llm.GenerateJsonAsync(_options.SystemPrompt, prompt, ct);
        if (!llmResult.IsSuccess)
        {
            return Result<CategorySuggestionResponse>.Failure(llmResult.ErrorCode ?? "LlmFailed", llmResult.ErrorMessage ?? "LLM failed.");
        }

        return ParseCategoryJson(llmResult.Value!);
    }

    private string BuildPrompt(object payload)
    {
        var template = string.IsNullOrWhiteSpace(_options.UserPromptTemplate)
            ? "Choose the best category for the invoice. Return strict JSON with categoryId, confidence, rationale. Input: {{json}}"
            : _options.UserPromptTemplate;

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        return template.Replace("{{json}}", json);
    }

    private static Result<CategorySuggestionResponse> ParseCategoryJson(string json)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return Result<CategorySuggestionResponse>.Failure("ValidationError", "Invalid JSON.");
            }

            var id = root.TryGetProperty("categoryId", out var idElement)
                ? idElement.GetString()
                : null;
            var confidence = root.TryGetProperty("confidence", out var confElement)
                ? confElement.GetDouble()
                : 0d;
            var rationale = root.TryGetProperty("rationale", out var rationaleElement)
                ? rationaleElement.GetString() ?? string.Empty
                : string.Empty;

            Guid? parsedId = null;
            if (!string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out var guid))
            {
                parsedId = guid;
            }

            if (confidence < 0d || confidence > 1d)
            {
                return Result<CategorySuggestionResponse>.Failure("ValidationError", "Invalid confidence.");
            }

            return Result<CategorySuggestionResponse>.Success(new CategorySuggestionResponse
            {
                SuggestedCategoryId = parsedId,
                Confidence = confidence,
                Rationale = rationale
            });
        }
        catch (Exception ex)
        {
            return Result<CategorySuggestionResponse>.Failure("ValidationError", ex.Message);
        }
    }
}
