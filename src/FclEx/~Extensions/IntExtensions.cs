using System;
using System.Collections.Generic;
using System.Text;
using Dawn;

namespace FclEx
{
    public static class IntExtensions
    {
        public static int PageCount(this int total, int pageSize)
        {
            Guard.Argument(total, nameof(total)).NotNegative();
            Guard.Argument(pageSize, nameof(pageSize)).Positive();

            if (total == 0) return 0;
            return (total - 1) / pageSize + 1;
        }
    }
}
