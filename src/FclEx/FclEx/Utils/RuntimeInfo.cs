using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Utils
{
    public static class RuntimeInfo
    {
        public static bool IsDebuggerAttached { get; } = System.Diagnostics.Debugger.IsAttached;
    }
}
