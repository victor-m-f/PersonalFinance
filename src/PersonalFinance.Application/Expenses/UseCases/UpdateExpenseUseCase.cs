using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class UpdateExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateExpenseRequest> _validator;
    private readonly ILogger<UpdateExpenseUseCase> _logger;

    public UpdateExpenseUseCase(
        IExpenseRepository expenseRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateExpenseRequest> validator,
        ILogger<UpdateExpenseUseCase> logger)
    {
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UpdateExpenseRequest request, CancellationToken ct)
    {
        var validationResult = await ValidateRequestAsync();
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var expense = await _expenseRepository.GetByIdAsync(request.Id, ct);
        if (expense is null)
        {
            _logger.LogWarning("Expense not found when attempting update. Id {ExpenseId}", request.Id);
            return Result.Failure(Errors.NotFound, "Expense not found.");
        }

        var updateResult = expense.Update(request.Date, request.Amount, request.Description);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.ErrorCode!, updateResult.ErrorMessage!);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Expense updated successfully with Id {ExpenseId}", expense.Id);
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
                "UpdateExpense validation failed for Id {ExpenseId}. Error: {ErrorMessage}",
                request.Id,
                message);

            return Result.Failure(Errors.ValidationError, message);
        }
    }
}
