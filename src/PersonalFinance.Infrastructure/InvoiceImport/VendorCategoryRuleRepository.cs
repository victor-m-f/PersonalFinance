using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Data;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class VendorCategoryRuleRepository : IVendorCategoryRuleRepository
{
    private readonly PersonalFinanceDbContext _dbContext;

    public VendorCategoryRuleRepository(PersonalFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(VendorCategoryRule rule, CancellationToken ct)
    {
        await _dbContext.VendorCategoryRules.AddAsync(rule, ct);
    }

    public Task UpdateAsync(VendorCategoryRule rule, CancellationToken ct)
    {
        var tracked = _dbContext.VendorCategoryRules.Local.FirstOrDefault(x => x.Id == rule.Id);
        if (tracked is null)
        {
            _dbContext.Entry(rule).State = EntityState.Modified;
        }
        else
        {
            _dbContext.Entry(tracked).CurrentValues.SetValues(rule);
        }

        return Task.CompletedTask;
    }

    public async Task<VendorCategoryRule?> GetByKeywordAsync(string keywordNormalized, CancellationToken ct)
    {
        return await _dbContext.VendorCategoryRules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.KeywordNormalized == keywordNormalized, ct);
    }

    public async Task<IReadOnlyList<VendorCategoryRule>> GetAllAsync(CancellationToken ct)
    {
        return await _dbContext.VendorCategoryRules
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
