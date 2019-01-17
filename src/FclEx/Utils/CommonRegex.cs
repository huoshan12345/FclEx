using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FclEx.Utils
{
    public static class CommonRegex
    {
        public static Regex HostPort { get; } = new Regex(@"([^:]+)(?::(\d+))?", RegexOptions.Compiled);
        public static Regex Ipv6HostPort { get; } = new Regex(@"\[[^\[^\]]+\](?::(\d+))?", RegexOptions.Compiled);
        public static Regex Scheme { get; } = new Regex(@"(\S+)://", RegexOptions.Compiled);
    }
}
