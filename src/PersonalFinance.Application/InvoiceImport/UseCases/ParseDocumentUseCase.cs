using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.UseCases;

public sealed class ParseDocumentUseCase
{
    private static readonly Regex AmountRegex = new(@"(\d{1,3}(?:[\.\s]\d{3})*,\d{2})|(\d+\.\d{2})", RegexOptions.Compiled);
    private static readonly Regex DateRegex = new(@"(\d{1,2})/(\d{1,2})/(\d{2,4})", RegexOptions.Compiled);

    private readonly IImportedDocumentRepository _repository;
    private readonly IImportedDocumentStorage _storage;
    private readonly IDocumentTextExtractor _extractor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ParseDocumentRequest> _validator;
    private readonly ILogger<ParseDocumentUseCase> _logger;

    public ParseDocumentUseCase(
        IImportedDocumentRepository repository,
        IImportedDocumentStorage storage,
        IDocumentTextExtractor extractor,
        IUnitOfWork unitOfWork,
        IValidator<ParseDocumentRequest> validator,
        ILogger<ParseDocumentUseCase> logger)
    {
        _repository = repository;
        _storage = storage;
        _extractor = extractor;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ParseDocumentResponse>> ExecuteAsync(
        ParseDocumentRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<ParseDocumentResponse>.Failure(
                Errors.ValidationError,
                validation.Errors.First().ErrorMessage);
        }

        var document = await _repository.GetByIdAsync(request.DocumentId, ct);
        if (document is null)
        {
            return Result<ParseDocumentResponse>.Failure(Errors.NotFound, "Document not found.");
        }

        var openResult = await _storage.OpenReadAsync(document.StoredFileName, ct);
        if (!openResult.IsSuccess)
        {
            return Result<ParseDocumentResponse>.Failure(
                openResult.ErrorCode ?? Errors.Conflict,
                openResult.ErrorMessage ?? "Failed to open document.");
        }

        await using var stream = openResult.Value!;
        var extractionResult = await _extractor.ExtractAsync(new DocumentTextExtractionRequest
        {
            StoredFileName = document.StoredFileName,
            FileExtension = document.FileExtension,
            Content = stream
        }, ct);

        if (!extractionResult.IsSuccess)
        {
            document.MarkFailed(extractionResult.ErrorMessage ?? "Extraction failed.");
            await _repository.UpdateAsync(document, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<ParseDocumentResponse>.Failure(
                extractionResult.ErrorCode ?? Errors.Conflict,
                extractionResult.ErrorMessage ?? "Failed to extract text.");
        }

        var extraction = extractionResult.Value!;
        var items = BuildDraftItems(extraction.RawText);
        var overall = ComputeOverallConfidence(items);
        document.MarkParsed(extraction.IsOcrUsed);
        await _repository.UpdateAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ParseDocumentResponse>.Success(new ParseDocumentResponse
        {
            DocumentId = document.Id,
            IsOcrUsed = extraction.IsOcrUsed,
            RawText = extraction.RawText,
            Items = items.Select(item => new ExpenseDraftItemResponse
            {
                Date = item.Date,
                Amount = item.Amount,
                Description = item.Description,
                CategoryId = item.CategoryId,
                CategoryName = item.CategoryName,
                Confidence = item.Confidence.Value
            }).ToList(),
            OverallConfidence = overall
        });
    }

    private static IReadOnlyList<ExpenseDraftItem> BuildDraftItems(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return Array.Empty<ExpenseDraftItem>();
        }

        var dates = ExtractDates(rawText).ToList();
        var amounts = ExtractAmounts(rawText).ToList();
        if (amounts.Count == 0)
        {
            return Array.Empty<ExpenseDraftItem>();
        }

        var items = new List<ExpenseDraftItem>();
        var confidenceResult = ConfidenceScore.Create(0.6d);
        if (!confidenceResult.IsSuccess)
        {
            return Array.Empty<ExpenseDraftItem>();
        }

        var confidence = confidenceResult.Value!;
        for (var i = 0; i < amounts.Count; i++)
        {
            var date = dates.Count > i ? dates[i] : dates.FirstOrDefault();
            var itemResult = ExpenseDraftItem.Create(
                date == default ? DateTime.Today : date,
                amounts[i],
                null,
                null,
                null,
                confidence);

            if (itemResult.IsSuccess)
            {
                items.Add(itemResult.Value!);
            }
        }

        return items;
    }

    private static IEnumerable<DateTime> ExtractDates(string text)
    {
        foreach (Match match in DateRegex.Matches(text))
        {
            if (!match.Success)
            {
                continue;
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
                yield return date;
            }
        }
    }

    private static IEnumerable<decimal> ExtractAmounts(string text)
    {
        foreach (Match match in AmountRegex.Matches(text))
        {
            if (!match.Success)
            {
                continue;
            }

            var value = match.Value;
            if (TryParseAmount(value, out var amount))
            {
                yield return amount;
            }
        }
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

    private static int ParseInt(string value)
    {
        return int.TryParse(value, out var result) ? result : 0;
    }

    private static double ComputeOverallConfidence(IReadOnlyList<ExpenseDraftItem> items)
    {
        if (items.Count == 0)
        {
            return 0d;
        }

        return items.Average(item => item.Confidence.Value);
    }
}
