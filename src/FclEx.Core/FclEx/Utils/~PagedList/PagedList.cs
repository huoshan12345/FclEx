namespace FclEx.Utils;

public class PagedList
{
    public static PagedList<T> Create<T>(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
        => new(items, pageIndex, pageSize, totalCount);
}

public class PagedList<T> : IPagedList<T>
{
    private readonly T[] _items;

    public static PagedList<T> Empty { get; } = new([], 0, 1, 0);

    public PagedList(T item) : this([item], 0, 1, 1) { }

    public PagedList(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Check.NotNull(items);
        Check.NotLessThan(pageIndex, 0);
        Check.NotLessThan(pageSize, 1);
        Check.NotLessThan(totalCount, 0);

        var pageCount = totalCount == 0
            ? 0
            : (int)(((long)totalCount + pageSize - 1) / pageSize);
        if (pageIndex >= Math.Max(1, pageCount))
            throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "The page index must refer to an existing page.");

        var offset = (long)pageIndex * pageSize;
        var maximumItemCount = totalCount == 0
            ? 0
            : (int)Math.Min(pageSize, totalCount - offset);
        if (items.Count > maximumItemCount)
            throw new ArgumentException($"The page can contain at most {maximumItemCount} items.", nameof(items));

        _items = [..items];
        PageIndex = pageIndex;
        PageNumber = pageIndex + 1;
        TotalCount = totalCount;
        PageSize = pageSize;
        PageCount = pageCount;

        HasPreviousPage = PageIndex > 0;
        HasNextPage = PageNumber < PageCount;
        IsFirstPage = PageIndex <= 0;
        IsLastPage = PageNumber >= PageCount;

        ItemStart = _items.Length == 0 ? 0 : (int)offset + 1;
        ItemEnd = _items.Length == 0 ? 0 : (int)offset + _items.Length;
    }

    public int PageCount { get; }
    public int TotalCount { get; }
    public int PageIndex { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
    public bool IsFirstPage { get; }
    public bool IsLastPage { get; }
    public int ItemStart { get; }
    public int ItemEnd { get; }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => _items.Length;
    public T this[int index] => _items[index];
}
