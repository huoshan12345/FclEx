using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using FclEx.Utils;

namespace FclEx
{
    public static class IntPtrExtensions
    {
        public static ValueDisposable<IntPtr> AsDisposable(this IntPtr ptr)
        {
            return ptr.AsDisposable(Marshal.FreeHGlobal);
        }
    }
}
