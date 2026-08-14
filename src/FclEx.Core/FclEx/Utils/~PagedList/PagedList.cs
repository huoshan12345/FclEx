namespace FclEx.Utils;

public class PagedList
{
    public static PagedList<T> Create<T>(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
        => new(items, pageIndex, pageSize, totalCount);
}

public class PagedList<T> : IPagedList<T>
{
    private readonly IReadOnlyList<T> _items;

    public static PagedList<T> Empty { get; } = new([], 0, 1, 0);

    public PagedList(T item) : this([item], 0, 1, 1) { }

    public PagedList(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Check.NotNull(items);
        Check.NotLessThan(pageIndex, 0);
        Check.NotLessThan(pageSize, 1);
        Check.NotLessThan(totalCount, 0);

        if (pageIndex > (int.MaxValue - 1) / pageSize)
            throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "The page offset is too large.");

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

        var offset = PageIndex * PageSize;
        ItemStart = TotalCount == 0 ? 0 : offset + 1;
        ItemEnd = (int)Math.Min((long)offset + PageSize, TotalCount);
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

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => _items.Count;
    public T this[int index] => _items[index];
}
