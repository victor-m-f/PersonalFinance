using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Categories.UseCases;

public sealed class UpdateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateCategoryRequest> _validator;
    private readonly ILogger<UpdateCategoryUseCase> _logger;

    public UpdateCategoryUseCase(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateCategoryRequest> validator,
        ILogger<UpdateCategoryUseCase> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UpdateCategoryRequest request, CancellationToken ct)
    {
        var validationResult = await ValidateRequestAsync();
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var colorResult = CategoryColor.Create(request.ColorHex);
        if (!colorResult.IsSuccess)
        {
            return Result.Failure(colorResult.ErrorCode!, colorResult.ErrorMessage!);
        }

        var category = await _categoryRepository.GetByIdAsync(request.Id, ct);
        if (category is null)
        {
            _logger.LogWarning("Category not found when attempting update. Id {CategoryId}", request.Id);
            return Result.Failure(Errors.NotFound, "Category not found.");
        }

        var parentCheckResult = await ValidateParentChangeIfNeededAsync(category);
        if (!parentCheckResult.IsSuccess)
        {
            return parentCheckResult;
        }

        var applyResult = ApplyChanges(category, colorResult.Value);
        if (!applyResult.IsSuccess)
        {
            return applyResult;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Category updated successfully with Id {CategoryId}", category.Id);
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
                "UpdateCategory validation failed for Id {CategoryId}. Error: {ErrorMessage}",
                request.Id,
                message);

            return Result.Failure(Errors.ValidationError, message);
        }

        async Task<Result> ValidateParentChangeIfNeededAsync(Category category)
        {
            var requestedParentId = request.ParentId;

            var isChangingParent =
                requestedParentId != category.ParentId;

            if (!requestedParentId.HasValue || !isChangingParent)
            {
                return Result.Success();
            }

            if (!await _categoryRepository.ExistsAsync(requestedParentId.Value, ct))
            {
                _logger.LogWarning(
                    "Parent category not found when attempting update. CategoryId {CategoryId}, ParentId {ParentId}",
                    request.Id,
                    requestedParentId.Value);

                return Result.Failure(Errors.NotFound, "Parent category not found.");
            }

            if (await IsCycleAsync())
            {
                _logger.LogWarning(
                    "Update blocked due to cycle detection. CategoryId {CategoryId}, ParentId {ParentId}",
                    request.Id,
                    requestedParentId.Value);

                return Result.Failure(Errors.ValidationError, "Category cannot be its own ancestor.");
            }

            return Result.Success();
        }

        Result ApplyChanges(Category category, CategoryColor color)
        {
            var renameResult = category.Rename(request.Name);
            if (!renameResult.IsSuccess)
            {
                return Result.Failure(renameResult.ErrorCode!, renameResult.ErrorMessage!);
            }

            var colorSetResult = category.SetColor(color);
            if (!colorSetResult.IsSuccess)
            {
                return Result.Failure(colorSetResult.ErrorCode!, colorSetResult.ErrorMessage!);
            }

            var parentResult = category.SetParent(request.ParentId);
            if (!parentResult.IsSuccess)
            {
                return Result.Failure(parentResult.ErrorCode!, parentResult.ErrorMessage!);
            }

            return Result.Success();
        }

        async Task<bool> IsCycleAsync()
        {
            const int maxDepth = 200;

            var current = request.ParentId;
            var guard = new HashSet<Guid>();

            var depth = 0;

            while (current.HasValue)
            {
                depth++;
                if (depth > maxDepth)
                {
                    return true;
                }

                if (current.Value == request.Id)
                {
                    return true;
                }

                if (!guard.Add(current.Value))
                {
                    return true;
                }

                current = await _categoryRepository.GetParentIdAsync(current.Value, ct);
            }

            return false;
        }
    }
}