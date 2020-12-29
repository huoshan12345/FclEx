using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx
{
    public static class NameValueCollectionExtensions
    {
        public static bool IsEmpty(this NameValueCollection col)
        {
            return col.Count == 0;
        }
    }
}
