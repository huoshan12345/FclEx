using System.Collections.Specialized;
using System.Web;

namespace FclEx
{
    public static class StringExtensions
    {
        public static NameValueCollection ParseQueryString(this string query) => HttpUtility.ParseQueryString(query);
    }
}
