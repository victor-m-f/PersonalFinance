using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.UseCases;

public sealed class ConfirmImportUseCase
{
    private readonly IImportedDocumentRepository _repository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ConfirmImportRequest> _validator;
    private readonly ILogger<ConfirmImportUseCase> _logger;

    public ConfirmImportUseCase(
        IImportedDocumentRepository repository,
        IExpenseRepository expenseRepository,
        IUnitOfWork unitOfWork,
        IValidator<ConfirmImportRequest> validator,
        ILogger<ConfirmImportUseCase> logger)
    {
        _repository = repository;
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ConfirmImportResponse>> ExecuteAsync(
        ConfirmImportRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<ConfirmImportResponse>.Failure(
                Errors.ValidationError,
                validation.Errors.First().ErrorMessage);
        }

        var document = await _repository.GetByIdAsync(request.DocumentId, ct);
        if (document is null)
        {
            return Result<ConfirmImportResponse>.Failure(Errors.NotFound, "Document not found.");
        }

        var createdIds = new List<Guid>();
        foreach (var item in request.Items)
        {
            var expenseResult = Expense.Create(item.Date, item.Amount, item.Description, item.CategoryId);
            if (!expenseResult.IsSuccess)
            {
                return Result<ConfirmImportResponse>.Failure(
                    expenseResult.ErrorCode ?? Errors.ValidationError,
                    expenseResult.ErrorMessage ?? "Invalid expense.");
            }

            var expense = expenseResult.Value!;
            await _expenseRepository.AddAsync(expense, ct);
            createdIds.Add(expense.Id);
        }

        document.MarkConfirmed();
        await _repository.UpdateAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Imported {Count} expenses from document {DocumentId}", createdIds.Count, document.Id);

        return Result<ConfirmImportResponse>.Success(new ConfirmImportResponse
        {
            DocumentId = document.Id,
            CreatedExpensesCount = createdIds.Count,
            CreatedExpenseIds = createdIds
        });
    }
}
