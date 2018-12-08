namespace FclEx.Utils
{
    public class PagedListModel<T>
    {
        public static PagedListModel<T> Empty { get; } = new PagedListModel<T>(PagedList<T>.Empty);

        public PagedListModel(IPagedList<T> list)
        {
            List = list;
            Total = list.TotalItemCount;
            PageNumber = list.PageNumber;
            PageSize = list.PageSize;
            ItemStart = list.ItemStart;
            ItemEnd = list.ItemEnd;
        }

        public IPagedList<T> List { get; set; }
        public int Total { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long ItemStart { get; set; }
        public long ItemEnd { get; set; }
    }
}
