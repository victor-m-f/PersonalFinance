using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface IVendorCategoryRuleRepository
{
    public Task AddAsync(VendorCategoryRule rule, CancellationToken ct);
    public Task UpdateAsync(VendorCategoryRule rule, CancellationToken ct);
    public Task<VendorCategoryRule?> GetByKeywordAsync(string keywordNormalized, CancellationToken ct);
    public Task<IReadOnlyList<VendorCategoryRule>> GetAllAsync(CancellationToken ct);
}
