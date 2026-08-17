namespace FclEx.Extensions;

public static class ReadOnlyListExtensions
{
    public static bool TryGet<T>(this IReadOnlyList<T> list, int index, [NotNullWhen(true)] out T? value) where T : notnull
    {
        if (index >= 0 && index < list.Count)
        {
            value = list[index]!;
            return true;
        }

        value = default;
        return false;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? Get<T>(this IReadOnlyList<T> list, int index, T? defaultValue = default) where T : notnull
    {
        return list.TryGet(index, out var value)
            ? value
            : defaultValue;
    }

    public static T Sample<T>(this IReadOnlyList<T> list, Random? random = null)
    {
        return (random ?? Random.Shared).Sample(list);
    }

    /// <summary>
    /// Marshals each item in <paramref name="list"/> to its native-layout bytes and concatenates the results.
    /// </summary>
    /// <typeparam name="T">The structure type to marshal.</typeparam>
    /// <param name="list">The items to marshal.</param>
    /// <returns>The native-layout bytes for all items in enumeration order.</returns>
    /// <remarks>
    /// This is an interop snapshot, not a portable or persistent serialization format. For pointer-based marshal
    /// fields, such as <see cref="UnmanagedType.LPStr"/> and <see cref="UnmanagedType.LPArray"/>, the returned bytes
    /// contain process-local addresses rather than the pointed-to data. Those addresses can become invalid after this
    /// method returns and must not be persisted, sent across processes, or used for structural equality.
    /// </remarks>
    public static byte[] MarshalToBytes<T>(this IReadOnlyList<T> list)
    {
        Check.NotNull(list);
        typeof(T).EnsureMarshalable();

        if (list.IsEmpty())
            return [];

        var length = Marshal.SizeOf<T>();
        var totalBytes = length * list.Count;
        var bufByte = new byte[totalBytes];
        using var disposable = Marshal.AllocHGlobalDisposable(length);
        var ptr = disposable.Value;
        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            Check.NotNull(item, nameof(list) + $"[{i}]");

            var structureInitialized = false;
            try
            {
                Marshal.StructureToPtr(item, ptr, false);
                structureInitialized = true;
                Marshal.Copy(ptr, bufByte, i * length, length);
            }
            finally
            {
                if (structureInitialized)
                    Marshal.DestroyStructure<T>(ptr);
            }
        }

        return bufByte;
    }
}
