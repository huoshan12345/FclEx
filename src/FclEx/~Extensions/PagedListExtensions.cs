using System;
using System.Linq;
using FclEx.Utils;

namespace FclEx
{
    public static class PagedListExtensions
    {
        public static PagedListModel<T> ToModel<T>(this IPagedList<T> list)
        {
            return new PagedListModel<T>(list);
        }

        public static IPagedList<T2> ToPagedList<T1, T2>(this IPagedList<T1> list, Func<T1, T2> selector)
        {
            return new PagedList<T2>(list.Select(selector).ToArray(), list.PageIndex, list.PageSize, list.TotalItemCount);
        }
    }
}
