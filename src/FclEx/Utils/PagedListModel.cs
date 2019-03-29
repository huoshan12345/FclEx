namespace FclEx.Utils
{
    public class PagedListModel<T>
    {
        private IPagedList<T> _list = PagedList<T>.Empty;

        public PagedListModel(IPagedList<T> list = null)
        {
            List = list;
        }

        public IPagedList<T> List
        {
            get => _list;
            set => _list = value ?? _list;
        }

        public int Total => List.TotalItemCount;
        public int PageNumber => List.PageNumber;
        public int PageSize => List.PageSize;
        public long ItemStart => List.ItemStart;
        public long ItemEnd => List.ItemEnd;
    }

    public class PagedListModel<T, TSelf> : PagedListModel<T> 
        where TSelf : PagedListModel<T, TSelf>, new()
    {
        public static TSelf Empty { get; } = new TSelf();
    }
}
