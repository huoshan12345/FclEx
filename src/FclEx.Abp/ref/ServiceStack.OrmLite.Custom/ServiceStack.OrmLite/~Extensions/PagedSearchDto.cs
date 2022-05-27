using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceStack.OrmLite
{
    public interface IPagedSearchDto
    {
        int? PageNumber { get; set; }
        int? PageSize { get; set; }
    }

    public class PagedSearchDto : IPagedSearchDto
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}
