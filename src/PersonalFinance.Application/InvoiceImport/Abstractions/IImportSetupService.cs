using PersonalFinance.Application.InvoiceImport.Responses;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface IImportSetupService
{
    Task<IReadOnlyList<ImportSetupItemStatus>> GetStatusAsync(CancellationToken ct);
    Task DownloadAsync(string itemKey, IProgress<ImportSetupProgress> progress, CancellationToken ct);
}
