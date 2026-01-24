namespace PersonalFinance.Application.Abstractions.Paging;

public sealed record class PageRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 15;
    public const int MaxPageSize = 200;
    public const int MinPageSize = 1;

    public int PageNumber { get; init; } = DefaultPageNumber;
    public int PageSize { get; init; } = DefaultPageSize;

    public static PageRequest Create(int pageNumber = DefaultPageNumber, int pageSize = DefaultPageSize)
    {
        return new PageRequest
        {
            PageNumber = ClampPageNumber(pageNumber),
            PageSize = ClampPageSize(pageSize)
        };
    }

    public static PageRequest Normalize(PageRequest? request)
    {
        if (request is null)
        {
            return Create();
        }

        return Create(request.PageNumber, request.PageSize);
    }

    private static int ClampPageNumber(int pageNumber)
    {
        if (pageNumber < 1)
        {
            return 1;
        }

        return pageNumber;
    }

    private static int ClampPageSize(int pageSize)
    {
        if (pageSize < MinPageSize)
        {
            return MinPageSize;
        }

        if (pageSize > MaxPageSize)
        {
            return MaxPageSize;
        }

        return pageSize;
    }
}
