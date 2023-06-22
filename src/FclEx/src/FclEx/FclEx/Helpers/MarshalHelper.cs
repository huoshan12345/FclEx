using System.Runtime.InteropServices;
using FclEx.Extensions;

namespace FclEx.Helpers;

public static class MarshalHelper
{
    public static DisposableValue<IntPtr> AllocHGlobal(int cb)
    {
        return Marshal.AllocHGlobal(cb).AsDisposable();
    }
}