namespace TechSpherex.CleanArchitecture.Domain.Common;

public sealed class PagedResult<T>(List<T> items, int totalCount, int page, int pageSize)
{
    public List<T> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
