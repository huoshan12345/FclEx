using System.Collections;
using System.Collections.Generic;

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

public interface IPagedList<out T> : IEnumerable<T>, IPagedList
{

}

public class PagedList<T> : IPagedList<T>
{
    private readonly IEnumerable<T> _items;

    public static PagedList<T> Empty { get; } = new(Array.Empty<T>(), 0, 1, 0);

    public PagedList(T item) : this(new[] { item }, 0, 1, 1) { }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public PagedList(IEnumerable<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Check.NotNull(items);
        Check.NotLessThan(pageIndex, 0);
        Check.NotLessThan(totalCount, 0);

        if (pageSize < 1 && totalCount > 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Value can not be less than 1.");

        if (pageSize < 0 && totalCount == 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Value can not be less than 0.");

        _items = items;
        PageIndex = pageIndex;
        PageNumber = pageIndex + 1;
        TotalCount = totalCount;
        PageSize = pageSize;
        PageCount = TotalCount > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

        HasPreviousPage = PageIndex > 0;
        HasNextPage = PageNumber < PageCount;
        IsFirstPage = PageIndex <= 0;
        IsLastPage = PageNumber >= PageCount;

        ItemStart = TotalCount == 0 ? 0 : PageIndex * PageSize + 1;
        ItemEnd = Math.Min(PageIndex * PageSize + PageSize, TotalCount);
    }

    public int PageCount { get; private set; }
    public int TotalCount { get; private set; }
    public int PageIndex { get; private set; }
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public bool HasPreviousPage { get; private set; }
    public bool HasNextPage { get; private set; }
    public bool IsFirstPage { get; private set; }
    public bool IsLastPage { get; private set; }
    public int ItemStart { get; private set; }
    public int ItemEnd { get; private set; }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}