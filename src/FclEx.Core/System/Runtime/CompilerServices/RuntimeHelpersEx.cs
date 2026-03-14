namespace System.Runtime.CompilerServices;

internal static class RuntimeHelpersEx
{
#if NETSTANDARD2_0
    public static bool IsReferenceOrContainsReferences<T>()
    {
        return true; // fallback
    }
#else
    public static bool IsReferenceOrContainsReferences<T>()
        => RuntimeHelpers.IsReferenceOrContainsReferences<T>();
#endif
}

