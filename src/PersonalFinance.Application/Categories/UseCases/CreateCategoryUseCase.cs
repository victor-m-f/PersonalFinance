using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.Responses;
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

    public Task<Result<CategoryIdResponse>> ExecuteAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result<CategoryIdResponse>.Failure(Errors.ValidationError, "Not implemented."));
    }
}
