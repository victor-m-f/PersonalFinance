using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.Responses;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class CreateExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateExpenseRequest> _validator;
    private readonly ILogger<CreateExpenseUseCase> _logger;

    public CreateExpenseUseCase(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateExpenseRequest> validator,
        ILogger<CreateExpenseUseCase> logger)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ExpenseIdResponse>> ExecuteAsync(CreateExpenseRequest request, CancellationToken ct)
    {
        var expenseResult = await CreateAndValidateExpenseAsync();

        if (!expenseResult.IsSuccess)
        {
            return Result<ExpenseIdResponse>.Failure(
                expenseResult.ErrorCode!,
                expenseResult.ErrorMessage!);
        }

        var expense = expenseResult.Value;

        if (expense.CategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(expense.CategoryId.Value, ct))
            {
                _logger.LogWarning(
                    "Category not found when creating expense. CategoryId {CategoryId}",
                    expense.CategoryId.Value);

                return Result<ExpenseIdResponse>.Failure(Errors.NotFound, "Category not found.");
            }
        }

        await _expenseRepository.AddAsync(expense, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Expense created successfully with Id {ExpenseId}",
            expense.Id);

        return Result<ExpenseIdResponse>.Success(
            new ExpenseIdResponse() { Id = expense.Id });

        async Task<Result<Expense>> CreateAndValidateExpenseAsync()
        {
            var validation = await _validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                return Result<Expense>.Failure(
                    Errors.ValidationError,
                    validation.Errors.First().ErrorMessage);
            }

            var expenseResult = Expense.Create(
                request.Date,
                request.Amount,
                request.Description,
                request.CategoryId);

            if (!expenseResult.IsSuccess)
            {
                return Result<Expense>.Failure(
                    expenseResult.ErrorCode!,
                    expenseResult.ErrorMessage!);
            }

            return Result<Expense>.Success(expenseResult.Value);
        }
    }
}
