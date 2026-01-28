using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Services;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.UseCases;

public sealed class AddVendorCategoryRuleUseCase
{
    private readonly IVendorCategoryRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AddVendorCategoryRuleRequest> _validator;
    private readonly ILogger<AddVendorCategoryRuleUseCase> _logger;

    public AddVendorCategoryRuleUseCase(
        IVendorCategoryRuleRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<AddVendorCategoryRuleRequest> validator,
        ILogger<AddVendorCategoryRuleUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(AddVendorCategoryRuleRequest request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result.Failure(Errors.ValidationError, validation.Errors.First().ErrorMessage);
        }

        var normalized = KeywordNormalizer.Normalize(request.Keyword);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Result.Failure(Errors.ValidationError, "Keyword is required.");
        }

        var confidenceResult = ConfidenceScore.Create(request.Confidence);
        if (!confidenceResult.IsSuccess)
        {
            return Result.Failure(confidenceResult.ErrorCode!, confidenceResult.ErrorMessage!);
        }

        var existing = await _repository.GetByKeywordAsync(normalized, ct);
        if (existing is not null)
        {
            var updateCategory = existing.UpdateCategory(request.CategoryId);
            if (!updateCategory.IsSuccess)
            {
                return updateCategory;
            }

            var updateConfidence = existing.UpdateConfidence(confidenceResult.Value!);
            if (!updateConfidence.IsSuccess)
            {
                return updateConfidence;
            }

            existing.MarkUsed();
            await _repository.UpdateAsync(existing, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("Updated vendor rule for {Keyword}", normalized);
            return Result.Success();
        }

        var ruleResult = VendorCategoryRule.Create(
            request.Keyword,
            normalized,
            request.CategoryId,
            confidenceResult.Value!);

        if (!ruleResult.IsSuccess)
        {
            return Result.Failure(ruleResult.ErrorCode!, ruleResult.ErrorMessage!);
        }

        await _repository.AddAsync(ruleResult.Value!, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created vendor rule for {Keyword}", normalized);
        return Result.Success();
    }
}
