using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using FclEx.Http.Core;

namespace FclEx.Http
{
    public static class Other
    {
        public static StringBuilder AppendHttpLine(this StringBuilder sb, string value)
        {
            return sb.Append(value + HttpConstants.NewLine);
        }
    }
}
