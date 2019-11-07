using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Environment;

namespace FclEx.Utils
{
    public class SimpleException : Exception
    {
        private static char[] NewLineChars { get; } = NewLine.ToCharArray();

        private static string[] SkipMethodNames { get; } =
        {
            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder",
            "System.Runtime.CompilerServices.AsyncMethodBuilderCore",
            "System.Threading.Tasks.Task`1.InnerInvoke",
            "System.Threading.ExecutionContext.RunInternal",
            "System.Threading.Tasks.Task.ExecuteWithThreadLocal",
            "System.Threading.Tasks.Task.ExecuteEntry",
            "System.Threading.Tasks.SynchronizationContextTaskScheduler"
        };

        private static string GetStackTrace()
        {
            var sb = new StringBuilder(byte.MaxValue);
            var stackTrace = new StackTrace(3);
            var lines = stackTrace.ToString().Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
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

        public SimpleException(string msg) : this(msg, GetStackTrace())
        {
        }

        public SimpleException(string msg, Exception inner) : this(msg, GetStackTrace(), inner)
        {
        }


        public SimpleException(string msg, string stackTrace) : base(msg)
        {
            StackTrace = stackTrace ?? GetStackTrace();
        }

        public SimpleException(string msg, string stackTrace, Exception inner) : base(msg, inner)
        {
            StackTrace = stackTrace ?? GetStackTrace();
        }

        public override string StackTrace { get; }

        public override string ToString()
        {
            var sb = new StringBuilder(GetType().ShortName(), 256);
            sb.AppendLineIf(() => ": " + Message, !Message.IsNullOrEmpty());
            var p = InnerException;
            while (p != null)
            {
                sb.AppendLine(" ---> " + p.GetType().ShortName());
                p = p.InnerException;
            }
            sb.AppendLineIf(StackTrace, !StackTrace.IsNullOrEmpty());
            return sb.ToString();
        }
    }
}
