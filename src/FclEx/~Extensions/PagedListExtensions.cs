using FclEx.Utils;

namespace FclEx
{
    public static class PagedListExtensions
    {
        public static PagedListModel<T> ToModel<T>(this IPagedList<T> list)
        {
            return new PagedListModel<T>(list);
        }
    }
}
