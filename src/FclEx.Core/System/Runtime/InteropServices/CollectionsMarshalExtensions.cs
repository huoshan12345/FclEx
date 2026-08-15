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

        /// <summary>
        /// Returns a writable span over the elements currently stored in an array-based collection.
        /// </summary>
        /// <remarks>
        /// Writing through the returned span bypasses collection validation and does not update its version. The caller
        /// must preserve every invariant imposed by the concrete collection, such as heap order or sorted order. The span
        /// is invalidated by any operation that changes the collection's count or capacity.
        /// </remarks>
        public static Span<T> AsSpan<TSelf, T>(ArrayBasedCollection<TSelf, T>? collection)
            where TSelf : ArrayBasedCollection<TSelf, T>
        {
            return collection is null
                ? default
                : collection.AsSpan();
        }
    }
}
