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