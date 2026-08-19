namespace FclEx.Extensions;

public static class RuntimeHelpersExtensions
{
    extension(RuntimeHelpers)
    {
        public static T GetUninitializedObject<T>()
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }

#if !NET5_0_OR_GREATER
        public static bool IsReferenceOrContainsReferences<T>()
        {
            return true; // fallback
        }

        public static object GetUninitializedObject(Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
        }
#endif
    }
}

