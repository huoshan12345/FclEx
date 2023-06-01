using System.Runtime.InteropServices;
using FclEx.Extensions;

namespace FclEx.Helpers;

public static class MarshalHelper
{
    public static ValueDisposable<IntPtr> AllocHGlobal(int cb)
    {
        return Marshal.AllocHGlobal(cb).AsDisposable();
    }
}