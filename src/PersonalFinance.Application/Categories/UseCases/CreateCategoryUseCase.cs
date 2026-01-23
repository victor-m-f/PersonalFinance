using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.Responses;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Categories.UseCases;

public sealed class CreateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCategoryRequest> _validator;
    private readonly ILogger<CreateCategoryUseCase> _logger;

    public CreateCategoryUseCase(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCategoryRequest> validator,
        ILogger<CreateCategoryUseCase> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CategoryIdResponse>> ExecuteAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        var categoryResult = await CreateAndValidateCategoryAsync();

        if (!categoryResult.IsSuccess)
        {
            return Result<CategoryIdResponse>.Failure(
                categoryResult.ErrorCode!,
                categoryResult.ErrorMessage!);
        }

        var category = categoryResult.Value;

        await _categoryRepository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Category created successfully with Id {CategoryId}",
            category.Id);

        return Result<CategoryIdResponse>.Success(
            new CategoryIdResponse() { Id = category.Id });

        async Task<Result<Category>> CreateAndValidateCategoryAsync()
        {
            var validation = await _validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return Result<Category>.Failure(
                    Errors.ValidationError,
                    validation.Errors[0].ErrorMessage);
            }

            var colorResult = CategoryColor.Create(request.ColorHex);
            if (!colorResult.IsSuccess)
            {
                return Result<Category>.Failure(
                    colorResult.ErrorCode!,
                    colorResult.ErrorMessage!);
            }

            var categoryResult = Category.Create(
                request.Name,
                colorResult.Value,
                request.ParentId);

            if (!categoryResult.IsSuccess)
            {
                return Result<Category>.Failure(
                    categoryResult.ErrorCode!,
                    categoryResult.ErrorMessage!);
            }

            return Result<Category>.Success(categoryResult.Value);
        }
    }
}
