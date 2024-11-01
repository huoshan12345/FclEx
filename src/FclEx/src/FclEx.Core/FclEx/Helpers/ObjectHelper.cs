namespace FclEx.Helpers;

public static class ObjectHelper
{
    public static object GetUninitializedObject(Type type)
    {
#if NETSTANDARD2_0
        return FormatterServices.GetUninitializedObject(type);
#else
        return RuntimeHelpers.GetUninitializedObject(type);
#endif
    }
}