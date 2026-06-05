namespace FclEx.Helpers;

public static class MarshalHelper
{
    public static DisposableValue<IntPtr> AllocHGlobal(int cb)
    {
        return Marshal.AllocHGlobal(cb).ToDisposable(Marshal.FreeHGlobal);
    }

    public static DisposableValue<IntPtr> SecureStringToBSTR(SecureString str)
    {
        return Marshal.SecureStringToBSTR(str).ToDisposable(Marshal.ZeroFreeBSTR);
    }
}