using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Environment;

namespace FclEx.Utils
{
    public class SimpleException : Exception
    {
        private static string GetStackTrace()
        {
            return EnhancedStackTrace.Current().ToString();
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
            sb.AppendIf(() => ": " + Message, !Message.IsNullOrEmpty());
            sb.AppendIf(() => " ---> " + InnerException, InnerException != null);
            sb.AppendIf(NewLine + StackTrace, !StackTrace.IsNullOrEmpty());
            return sb.ToString();
        }
    }
}
