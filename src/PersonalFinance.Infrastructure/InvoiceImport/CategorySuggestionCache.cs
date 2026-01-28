using Microsoft.Extensions.Options;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Application.InvoiceImport.Settings;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class CategorySuggestionCache : ICategorySuggestionCache, ICategorySuggestionCacheInvalidator
{
    private readonly ICategorySuggestionRepository _repository;
    private readonly CategorySuggestionOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IReadOnlyList<CategorySuggestionCategoryResponse> _cache = Array.Empty<CategorySuggestionCategoryResponse>();
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

    public CategorySuggestionCache(
        ICategorySuggestionRepository repository,
        IOptions<CategorySuggestionOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<CategorySuggestionCategoryResponse>> GetCategoriesAsync(CancellationToken ct)
    {
        if (IsFresh())
        {
            return _cache;
        }

        await _gate.WaitAsync(ct);
        try
        {
            if (!IsFresh())
            {
                _cache = await _repository.GetCategoriesAsync(ct);
                _lastRefresh = DateTimeOffset.UtcNow;
            }

            return _cache;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Invalidate()
    {
        _lastRefresh = DateTimeOffset.MinValue;
    }

    private bool IsFresh()
    {
        if (_cache.Count == 0)
        {
            return false;
        }

        var ttl = TimeSpan.FromSeconds(Math.Max(30, _options.CacheDurationSeconds));
        return DateTimeOffset.UtcNow - _lastRefresh <= ttl;
    }
}
