using FclEx;

namespace FclEx.Extensions
{
    public static class IntExtensions
    {
        public static int PageCount(this int total, int pageSize)
        {
            Check.NotNegative(total);
            Check.Positive(pageSize);

            if (total == 0) return 0;
            return (total - 1) / pageSize + 1;
        }
    }
}
