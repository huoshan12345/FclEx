namespace FclEx.Helpers;

public static class ObjectHelper
{
    public static T CreateObject<T>(params object[] args)
    {
        return typeof(T).CreateObject(args).CastTo<T>();
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        (a, b) = (b, a);
    }
}