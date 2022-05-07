using System.Runtime.InteropServices;
using FclEx;
using FclEx.Utils;

namespace System.Reflection.InteropServices
{
    public static class MarshalHelper
    {
        public static ValueDisposable<IntPtr> AllocHGlobal(int cb)
        {
            return Marshal.AllocHGlobal(cb).AsDisposable();
        }
    }
}
