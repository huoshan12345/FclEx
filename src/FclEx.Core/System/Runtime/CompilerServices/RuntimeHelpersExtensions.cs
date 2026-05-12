namespace System.Runtime.CompilerServices;

internal static class RuntimeHelpersExtensions
{
    extension(RuntimeHelpers)
    {
#if !NET5_0_OR_GREATER
        public static bool IsReferenceOrContainsReferences<T>()
        {
            return true; // fallback
        }
#endif
    }
}

