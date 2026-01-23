using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Requests;
using PersonalFinance.Application.Categories.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Categories.UseCases;

public sealed class FilterCategoriesUseCase
{
    private readonly ICategoryReadRepository _categoryReadRepository;
    private readonly ILogger<FilterCategoriesUseCase> _logger;

    public FilterCategoriesUseCase(
        ICategoryReadRepository categoryReadRepository,
        ILogger<FilterCategoriesUseCase> logger)
    {
        _categoryReadRepository = categoryReadRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CategoryListItemResponse>>> ExecuteAsync(
        FilterCategoriesRequest request,
        CancellationToken ct)
    {
        var (name, sortBy, page) = GetClearFilters();

        _logger.LogInformation(
            "Filtering categories. HasNameFilter {HasNameFilter}, IsPaged {IsPaged}, SortBy {SortBy}, SortDescending {SortDescending}",
            name is not null,
            page is not null,
            sortBy,
            request.SortDescending);


        var result = await _categoryReadRepository.FilterAsync(
            request.ParentId,
            name,
            sortBy,
            request.SortDescending,
            page,
            ct);

        return Result<PagedResult<CategoryListItemResponse>>.Success(result);

        (string? name, string? sortBy, PageRequest? page) GetClearFilters()
        {
            var name = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = null;
            }

            var sortBy = request.SortBy?.Trim();
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                sortBy = null;
            }

            var page = request.Page is null ? null : PageRequest.Normalize(request.Page);

            return (name, sortBy, page);
        }
    }
}
