namespace FclEx.Utils;

public static class PagedListModel
{
    public static PagedListModel<T> Create<T>(IPagedList<T> list) => new(list);
    public static PagedListModel<T> Create<T>(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
        => new(PagedList.Create(items, pageIndex, pageSize, totalCount));
}

public class PagedListModel<T> : IPagedList
{
    private readonly IPagedList<T> _items;

    public PagedListModel(IPagedList<T>? list = null)
    {
        _items = list ?? PagedList<T>.Empty;
    }

    public IReadOnlyList<T> Items => _items;
    public int PageCount => _items.PageCount;
    public int TotalCount => _items.TotalCount;
    public int PageIndex => _items.PageIndex;
    public int PageNumber => _items.PageNumber;
    public int PageSize => _items.PageSize;
    public bool HasPreviousPage => _items.HasPreviousPage;
    public bool HasNextPage => _items.HasNextPage;
    public bool IsFirstPage => _items.IsFirstPage;
    public bool IsLastPage => _items.IsLastPage;
    public int ItemStart => _items.ItemStart;
    public int ItemEnd => _items.ItemEnd;

    public static readonly PagedListModel<T> Empty = new();
}