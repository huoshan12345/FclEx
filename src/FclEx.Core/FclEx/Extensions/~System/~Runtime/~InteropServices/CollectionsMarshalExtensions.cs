namespace FclEx.Extensions;

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
        /// Returns the internal capacity array used by a <see cref="List{T}"/>.
        /// </summary>
        /// <remarks>
        /// The returned array includes unused capacity slots. Writing it bypasses the list's count and version tracking,
        /// and any operation that replaces the list's backing storage detaches the returned array from the list. This API
        /// depends on the runtime's private <see cref="List{T}"/> layout and should only be used by carefully audited
        /// low-level code.
        /// </remarks>
        public static T[] Items<T>(List<T> list)
        {
            Check.NotNull(list);
            return ListAccessor<T>.Items(list);
        }

#if !NET8_0_OR_GREATER
        /// <summary>
        /// Sets the number of elements contained in a <see cref="List{T}"/> without adding or removing them individually.
        /// </summary>
        /// <remarks>
        /// Increasing the count exposes the current contents of previously unused capacity slots and does not initialize
        /// them through normal list insertion. This is a low-level API; callers must initialize newly exposed elements and
        /// preserve all list invariants before the list is observed elsewhere.
        /// </remarks>
        public static void SetCount<T>(List<T> list, int count)
        {
            Check.NotNull(list);
            Check.NotNegative(count);

            ref var size = ref ListAccessor<T>.Size(list);
            if (count > list.Capacity)
            {
                list.Capacity = count;
            }
            else if (count < size)
            {
                var items = ListAccessor<T>.Items(list);
                Array.Clear(items, count, size - count);
            }

            ref var version = ref ListAccessor<T>.Version(list);
            ++version;
            size = count;
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
