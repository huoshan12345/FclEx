using System;
using System.Runtime.InteropServices;
using FclEx.Utils;

namespace FclEx.Helpers
{
    public static class MarshalHelper
    {
        public static ValueDisposable<IntPtr> AllocHGlobal(int cb)
        {
            return Marshal.AllocHGlobal(cb).AsDisposable();
        }
    }
}
