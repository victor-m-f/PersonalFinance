using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Infrastructure.Data.Repositories;

public sealed class ImportedDocumentRepository : RepositoryBase<ImportedDocument>, IImportedDocumentRepository
{
    public ImportedDocumentRepository(PersonalFinanceDbContext dbContext)
        : base(dbContext) { }

    public async Task AddAsync(ImportedDocument document, CancellationToken ct)
    {
        await Set.AddAsync(document, ct);
    }

    public async Task UpdateAsync(ImportedDocument document, CancellationToken ct)
    {
        var tracked = Set.Local.FirstOrDefault(x => x.Id == document.Id);
        if (tracked is null)
        {
            DbContext.Entry(document).State = EntityState.Modified;
        }
        else
        {
            DbContext.Entry(tracked).CurrentValues.SetValues(document);
        }
        await Task.CompletedTask;
    }

    public async Task<ImportedDocument?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await Set
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<ImportedDocument?> GetByHashAsync(string hash, CancellationToken ct)
    {
        var hashValue = DocumentHash.FromStorage(hash);
        return await Set
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Hash == hashValue, ct);
    }
}
