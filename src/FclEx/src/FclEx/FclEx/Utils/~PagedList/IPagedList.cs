namespace FclEx.Utils;

public interface IPagedList
{
    int PageCount { get; }
    int TotalCount { get; }
    int PageIndex { get; }
    int PageNumber { get; }
    int PageSize { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
    bool IsFirstPage { get; }
    bool IsLastPage { get; }
    int ItemStart { get; }
    int ItemEnd { get; }
}

public interface IPagedList<out T> : IReadOnlyList<T>, IPagedList
{
}