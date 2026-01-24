using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class DeleteExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IValidator<DeleteExpenseRequest> _validator;
    private readonly ILogger<DeleteExpenseUseCase> _logger;

    public DeleteExpenseUseCase(
        IExpenseRepository expenseRepository,
        IValidator<DeleteExpenseRequest> validator,
        ILogger<DeleteExpenseUseCase> logger)
    {
        _expenseRepository = expenseRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(DeleteExpenseRequest request, CancellationToken ct)
    {
        var validationResult = await ValidateRequestAsync();
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        if (!await _expenseRepository.ExistsAsync(request.Id, ct))
        {
            _logger.LogWarning(
                "Expense not found when attempting deletion. Id {ExpenseId}",
                request.Id);

            return Result.Failure(Errors.NotFound, "Expense not found.");
        }

        await _expenseRepository.DeleteByIdAsync(request.Id, ct);

        _logger.LogInformation(
            "Expense deleted successfully with Id {ExpenseId}",
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
                "DeleteExpense validation failed for Id {ExpenseId}. Error: {ErrorMessage}",
                request.Id,
                message);

            return Result.Failure(Errors.ValidationError, message);
        }
    }
}
