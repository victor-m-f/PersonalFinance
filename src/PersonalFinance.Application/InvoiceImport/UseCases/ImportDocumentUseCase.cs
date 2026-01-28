using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.UseCases;

public sealed class ImportDocumentUseCase
{
    private readonly IImportedDocumentRepository _repository;
    private readonly IImportedDocumentStorage _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<ImportDocumentRequest> _validator;
    private readonly ILogger<ImportDocumentUseCase> _logger;

    public ImportDocumentUseCase(
        IImportedDocumentRepository repository,
        IImportedDocumentStorage storage,
        IUnitOfWork unitOfWork,
        IValidator<ImportDocumentRequest> validator,
        ILogger<ImportDocumentUseCase> logger)
    {
        _repository = repository;
        _storage = storage;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ImportDocumentResponse>> ExecuteAsync(
        ImportDocumentRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<ImportDocumentResponse>.Failure(
                Errors.ValidationError,
                validation.Errors.First().ErrorMessage);
        }

        var storageResult = await _storage.SaveAsync(
            request.SourceFilePath,
            request.OriginalFileName,
            ct);

        if (!storageResult.IsSuccess)
        {
            return Result<ImportDocumentResponse>.Failure(
                storageResult.ErrorCode ?? Errors.Conflict,
                storageResult.ErrorMessage ?? "Failed to store document.");
        }

        var stored = storageResult.Value!;
        var existing = await _repository.GetByHashAsync(stored.Hash, ct);
        if (existing is not null)
        {
            _logger.LogInformation("Duplicate document detected: {Hash}", stored.Hash);
            return Result<ImportDocumentResponse>.Failure(
                Errors.Conflict,
                "Document already imported.");
        }

        var hashResult = DocumentHash.Create(stored.Hash);
        if (!hashResult.IsSuccess)
        {
            return Result<ImportDocumentResponse>.Failure(
                hashResult.ErrorCode ?? Errors.ValidationError,
                hashResult.ErrorMessage ?? "Invalid hash.");
        }

        var documentResult = ImportedDocument.Create(
            request.OriginalFileName,
            stored.StoredFileName,
            stored.FileExtension,
            stored.FileSize,
            hashResult.Value!);

        if (!documentResult.IsSuccess)
        {
            return Result<ImportDocumentResponse>.Failure(
                documentResult.ErrorCode ?? Errors.ValidationError,
                documentResult.ErrorMessage ?? "Invalid document.");
        }

        var document = documentResult.Value!;
        await _repository.AddAsync(document, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ImportDocumentResponse>.Success(new ImportDocumentResponse
        {
            DocumentId = document.Id,
            Hash = document.Hash.Value,
            Status = document.Status,
            CreatedAt = document.CreatedAt
        });
    }
}
