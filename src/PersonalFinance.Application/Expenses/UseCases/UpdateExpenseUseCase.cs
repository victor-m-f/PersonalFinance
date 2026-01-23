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

    public Task<Result> ExecuteAsync(UpdateExpenseRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Failure(Errors.ValidationError, "Not implemented."));
    }
}
