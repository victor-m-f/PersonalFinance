using System.Security.Cryptography;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Infrastructure.Documents;

public sealed class LocalImportedDocumentStorage : IImportedDocumentStorage
{
    private readonly string _rootFolder;

    public LocalImportedDocumentStorage()
    {
        _rootFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PersonalFinance",
            "imports");
    }

    public async Task<Result<StoredDocumentResponse>> SaveAsync(
        string sourceFilePath,
        string originalFileName,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
        {
            return Result<StoredDocumentResponse>.Failure("NotFound", "Source file not found.");
        }

        Directory.CreateDirectory(_rootFolder);

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = Path.GetExtension(sourceFilePath);
        }

        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".bin" : extension.ToLowerInvariant();
        var storedFileName = BuildStoredFileName(safeExtension);
        var destinationPath = Path.Combine(_rootFolder, storedFileName);

        try
        {
            await using var source = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var destination = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var hash = await CopyAndHashAsync(source, destination, ct);

            var fileInfo = new FileInfo(destinationPath);
            return Result<StoredDocumentResponse>.Success(new StoredDocumentResponse
            {
                StoredFileName = storedFileName,
                FileExtension = safeExtension.TrimStart('.'),
                FileSize = fileInfo.Length,
                Hash = hash
            });
        }
        catch (Exception ex)
        {
            return Result<StoredDocumentResponse>.Failure("StorageError", ex.Message);
        }
    }

    public Task<Result<Stream>> OpenReadAsync(string storedFileName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            return Task.FromResult(Result<Stream>.Failure("ValidationError", "Stored file name is required."));
        }

        var path = Path.Combine(_rootFolder, storedFileName);
        if (!File.Exists(path))
        {
            return Task.FromResult(Result<Stream>.Failure("NotFound", "Stored file not found."));
        }

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(Result<Stream>.Success(stream));
    }

    private static string BuildStoredFileName(string extension)
    {
        return $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
    }

    private static async Task<string> CopyAndHashAsync(Stream source, Stream destination, CancellationToken ct)
    {
        using var sha = SHA256.Create();
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            sha.TransformBlock(buffer, 0, bytesRead, null, 0);
        }

        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(sha.Hash ?? Array.Empty<byte>()).ToLowerInvariant();
    }
}
