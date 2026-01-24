using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class AssignExpenseCategoryUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AssignExpenseCategoryRequest> _validator;
    private readonly ILogger<AssignExpenseCategoryUseCase> _logger;

    public AssignExpenseCategoryUseCase(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<AssignExpenseCategoryRequest> validator,
        ILogger<AssignExpenseCategoryUseCase> logger)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(AssignExpenseCategoryRequest request, CancellationToken ct)
    {
        var validationResult = await ValidateRequestAsync();
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var expense = await _expenseRepository.GetByIdAsync(request.Id, ct);
        if (expense is null)
        {
            _logger.LogWarning("Expense not found when attempting category assign. Id {ExpenseId}", request.Id);
            return Result.Failure(Errors.NotFound, "Expense not found.");
        }

        if (request.CategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(request.CategoryId.Value, ct))
            {
                _logger.LogWarning(
                    "Category not found when assigning to expense. ExpenseId {ExpenseId}, CategoryId {CategoryId}",
                    request.Id,
                    request.CategoryId.Value);

                return Result.Failure(Errors.NotFound, "Category not found.");
            }
        }

        var assignResult = expense.AssignCategory(request.CategoryId);
        if (!assignResult.IsSuccess)
        {
            return Result.Failure(assignResult.ErrorCode!, assignResult.ErrorMessage!);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Expense category assigned successfully. ExpenseId {ExpenseId}",
            expense.Id);

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
                "AssignExpenseCategory validation failed for Id {ExpenseId}. Error: {ErrorMessage}",
                request.Id,
                message);

            return Result.Failure(Errors.ValidationError, message);
        }
    }
}
