namespace System.Runtime.CompilerServices;

internal static class RuntimeHelpersExtensions
{
    extension(RuntimeHelpers)
    {
#if NETSTANDARD2_0
        public static bool IsReferenceOrContainsReferences<T>()
        {
            return true; // fallback
        }
#endif
    }
}

