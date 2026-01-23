using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
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

    public Task<Result> ExecuteAsync(UpdateCategoryRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Failure(Errors.ValidationError, "Not implemented."));
    }
}
