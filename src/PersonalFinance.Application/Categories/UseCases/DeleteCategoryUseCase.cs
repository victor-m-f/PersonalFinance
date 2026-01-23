using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Categories.UseCases;

public sealed class DeleteCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteCategoryRequest> _validator;
    private readonly ILogger<DeleteCategoryUseCase> _logger;

    public DeleteCategoryUseCase(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<DeleteCategoryRequest> validator,
        ILogger<DeleteCategoryUseCase> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public Task<Result> ExecuteAsync(DeleteCategoryRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Failure(Errors.ValidationError, "Not implemented."));
    }
}
