using System;
using System.Collections.Generic;

namespace FclEx.Utils
{
    public interface IPagedList
    {
        int PageCount { get; }
        int TotalItemCount { get; }
        int PageIndex { get; }
        int PageNumber { get; }
        int PageSize { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
        long ItemStart { get; }
        long ItemEnd { get; }
    }

    public interface IPagedList<T> : IList<T>, IPagedList
    {

    }

    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public static PagedList<T> Empty { get; } = new PagedList<T>(Array.Empty<T>(), 0, 1, 0);

        public PagedList(T item) : this(new[] { item }, 0, 1, 1)
        {

        }

        public PagedList(ICollection<T> items, int pageIndex, int pageSize, int totalItemCount)
            : base(items)
        {
            Check.AtLeast(pageIndex, nameof(pageIndex), 0);
            Check.AtLeast(totalItemCount, nameof(totalItemCount), 0);

            if (pageSize < 1 && totalItemCount > 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Value can not be less than 1.");

            if (pageSize < 0 && totalItemCount == 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Value can not be less than 0.");

            PageIndex = pageIndex;
            PageNumber = pageIndex + 1;
            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            TotalItemCount = Math.Max(totalItemCount, items.Count);
            PageCount = TotalItemCount > 0 ? (int)Math.Ceiling(TotalItemCount / (double)PageSize) : 0;

            HasPreviousPage = PageIndex > 0;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageIndex <= 0;
            IsLastPage = PageNumber >= PageCount;

            ItemStart = TotalItemCount == 0 ? 0 : PageIndex * PageSize + 1;
            ItemEnd = Math.Min(PageIndex * PageSize + PageSize, TotalItemCount);
        }

        public int PageCount { get; private set; }
        public int TotalItemCount { get; private set; }
        public int PageIndex { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public bool HasPreviousPage { get; private set; }
        public bool HasNextPage { get; private set; }
        public bool IsFirstPage { get; private set; }
        public bool IsLastPage { get; private set; }
        public long ItemStart { get; private set; }
        public long ItemEnd { get; private set; }
    }
}
