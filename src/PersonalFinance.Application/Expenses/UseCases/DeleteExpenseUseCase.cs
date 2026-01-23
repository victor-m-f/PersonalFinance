using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class DeleteExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteExpenseRequest> _validator;
    private readonly ILogger<DeleteExpenseUseCase> _logger;

    public DeleteExpenseUseCase(
        IExpenseRepository expenseRepository,
        IUnitOfWork unitOfWork,
        IValidator<DeleteExpenseRequest> validator,
        ILogger<DeleteExpenseUseCase> logger)
    {
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public Task<Result> ExecuteAsync(DeleteExpenseRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Failure(Errors.ValidationError, "Not implemented."));
    }
}
