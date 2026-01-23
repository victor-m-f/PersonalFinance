using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class AssignExpenseCategoryUseCase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AssignExpenseCategoryRequest> _validator;
    private readonly ILogger<AssignExpenseCategoryUseCase> _logger;

    public AssignExpenseCategoryUseCase(
        IExpenseRepository expenseRepository,
        IUnitOfWork unitOfWork,
        IValidator<AssignExpenseCategoryRequest> validator,
        ILogger<AssignExpenseCategoryUseCase> logger)
    {
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public Task<Result> ExecuteAsync(AssignExpenseCategoryRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Failure(Errors.ValidationError, "Not implemented."));
    }
}
