using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FclEx.Utils;

namespace FclEx.Helpers
{
    public static class MarshalHelper
    {
        public static IntPtrDisposable AllocHGlobal(int cb)
        {
            return Marshal.AllocHGlobal(cb).AsDisposable();
        }
    }
}
