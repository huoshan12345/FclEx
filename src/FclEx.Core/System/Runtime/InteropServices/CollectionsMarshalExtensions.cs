namespace System.Runtime.InteropServices;

#if !NET5_0_OR_GREATER
public static class CollectionsMarshal;
#endif

public static class CollectionsMarshalExtensions
{
    extension(CollectionsMarshal)
    {
#if !NET5_0_OR_GREATER
        public static Span<T> AsSpan<T>(List<T>? list)
        {
            return list.IsNullOrEmpty()
                ? default
                : ListAccessor<T>.Items(list).AsSpan(0, list.Count);
        }
#endif
    }
}
