namespace FclEx.Utils
{
    public class PagedListModel<T> : IPagedList
    {
        private IPagedList<T> _list = PagedList<T>.Empty;

        public PagedListModel(IPagedList<T>? list = null)
        {
            List = list ?? PagedList<T>.Empty;
        }

        public IPagedList<T> List
        {
            get => _list;
            set => _list = value ?? PagedList<T>.Empty;
        }

        public int PageCount => List.PageCount;
        public int TotalCount => List.TotalCount;
        public int PageIndex => List.PageIndex;
        public int PageNumber => List.PageNumber;
        public int PageSize => List.PageSize;
        public bool HasPreviousPage => List.HasPreviousPage;
        public bool HasNextPage => List.HasNextPage;
        public bool IsFirstPage => List.IsFirstPage;
        public bool IsLastPage => List.IsLastPage;
        public int ItemStart => List.ItemStart;
        public int ItemEnd => List.ItemEnd;
    }

    public class PagedListModel<T, TSelf> : PagedListModel<T>
        where TSelf : PagedListModel<T, TSelf>, new()
    {
        public static TSelf Empty { get; } = new TSelf();
    }
}
