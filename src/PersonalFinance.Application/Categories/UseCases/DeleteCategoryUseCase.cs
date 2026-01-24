using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Categories.UseCases;

public sealed class DeleteCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<DeleteCategoryRequest> _validator;
    private readonly ILogger<DeleteCategoryUseCase> _logger;

    public DeleteCategoryUseCase(
        ICategoryRepository categoryRepository,
        IValidator<DeleteCategoryRequest> validator,
        ILogger<DeleteCategoryUseCase> logger)
    {
        _categoryRepository = categoryRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(DeleteCategoryRequest request, CancellationToken ct)
    {
        var validationResult = await ValidateRequestAsync();
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var guardsResult = await EnsureCategoryCanBeDeletedAsync();
        if (!guardsResult.IsSuccess)
        {
            return guardsResult;
        }

        await _categoryRepository.DeleteByIdAsync(request.Id, ct);

        _logger.LogInformation(
            "Category deleted successfully with Id {CategoryId}",
            request.Id);

        return Result.Success();

        async Task<Result> ValidateRequestAsync()
        {
            var validation = await _validator.ValidateAsync(request, ct);
            if (validation.IsValid)
            {
                return Result.Success();
            }

            var message = validation.Errors.First().ErrorMessage;

            _logger.LogWarning(
                "DeleteCategory validation failed for Id {CategoryId}. Error: {ErrorMessage}",
                request.Id,
                message);

            return Result.Failure(Errors.ValidationError, message);
        }

        async Task<Result> EnsureCategoryCanBeDeletedAsync()
        {
            if (!await _categoryRepository.ExistsAsync(request.Id, ct))
            {
                _logger.LogWarning(
                    "Category not found when attempting deletion. Id {CategoryId}",
                    request.Id);

                return Result.Failure(Errors.NotFound, "Category not found.");
            }

            if (await _categoryRepository.HasChildrenAsync(request.Id, ct))
            {
                return Result.Failure(Errors.Conflict, "Category has subcategories. Delete or move them first.");
            }

            if (await _categoryRepository.IsInUseByExpensesAsync(request.Id, ct))
            {
                return Result.Failure(Errors.Conflict, "Category is in use by expenses and cannot be deleted.");
            }

            return Result.Success();
        }
    }
}