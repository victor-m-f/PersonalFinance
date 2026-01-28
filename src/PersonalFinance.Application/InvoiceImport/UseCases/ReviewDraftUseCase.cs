using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.UseCases;

public sealed class ReviewDraftUseCase
{
    private readonly IValidator<ReviewDraftRequest> _validator;
    private readonly ILogger<ReviewDraftUseCase> _logger;

    public ReviewDraftUseCase(
        IValidator<ReviewDraftRequest> validator,
        ILogger<ReviewDraftUseCase> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ReviewDraftResponse>> ExecuteAsync(
        ReviewDraftRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<ReviewDraftResponse>.Failure(
                Errors.ValidationError,
                validation.Errors.First().ErrorMessage);
        }

        _logger.LogInformation("Reviewing draft for document {DocumentId}", request.DocumentId);

        return Result<ReviewDraftResponse>.Success(new ReviewDraftResponse
        {
            DocumentId = request.DocumentId,
            Items = request.Items.Select(item => new ExpenseDraftItemResponse
            {
                Date = item.Date,
                Amount = item.Amount,
                Description = item.Description,
                CategoryId = item.CategoryId,
                CategoryName = item.CategoryName,
                Confidence = item.Confidence
            }).ToList()
        });
    }
}
