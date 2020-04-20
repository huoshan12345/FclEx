using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FclEx.Helpers;
using static System.Environment;

namespace FclEx.Utils
{
    public class SimpleException : Exception
    {
        private static char[] NewLineChars { get; } = NewLine.ToCharArray();

        private static string[] SkipMethodNames { get; } =
        {
            "System.Threading",
            "System.Runtime.CompilerServices",
        };

        private static string GetStackTrace()
        {
            using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
            var sb = disposable.Value;

            var stackTrace = new StackTrace(3);
            var lines = stackTrace.ToString().Split(NewLineChars).Where(m => !m.IsNullOrWhiteSpace());
            var lastLine = string.Empty;
            var count = 1;
            foreach (var line in lines)
            {
                if (SkipMethodNames.Any(m => line.Contains(m, StringComparison.Ordinal)))
                    continue;

                if (line != lastLine)
                {
                    var msg = count == 1 ? line : line + " *" + count;
                    sb.AppendLine(msg);
                    count = 1;
                    lastLine = line;
                }
                else
                {
                    ++count;
                }
            }
            if (count > 1)
            {
                sb.AppendLine(lastLine + " *" + count);
            }
            return sb.ToString();
        }

        public SimpleException(string? msg) : base(msg)
        {
            StackTrace = GetStackTrace();
        }
        
        public SimpleException(string? msg, Exception? inner) : base(msg, inner)
        {
            StackTrace = GetStackTrace();
        }

        public override string StackTrace { get; }

        public override string ToString()
        {
            using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
            var sb = disposable.Value;
            sb.Append(GetType().LongName());
            sb.AppendLine(Message.IsValid() ? ": " + Message : string.Empty);
            var p = InnerException;
            while (p != null)
            {
                sb.AppendLine(" ---> " + p.GetType().LongName());
                p = p.InnerException;
            }
            sb.AppendLineIf(StackTrace, !StackTrace.IsNullOrEmpty());
            return sb.ToString();
        }
    }
}
