using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
