using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class CreateExpenseUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateExpenseRequest> _validator;
    private readonly ILogger<CreateExpenseUseCase> _logger;

    public CreateExpenseUseCase(
        IExpenseRepository expenseRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateExpenseRequest> validator,
        ILogger<CreateExpenseUseCase> logger)
    {
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public Task<Result<ExpenseIdResponse>> ExecuteAsync(CreateExpenseRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result<ExpenseIdResponse>.Failure(Errors.ValidationError, "Not implemented."));
    }
}
