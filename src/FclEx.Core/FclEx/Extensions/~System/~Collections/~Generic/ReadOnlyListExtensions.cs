namespace FclEx.Extensions;

public static class ReadOnlyListExtensions
{
    public static bool TryGet<T>(this IReadOnlyList<T> list, int index, [NotNullWhen(true)] out T? value)
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
    public static T? Get<T>(this IReadOnlyList<T> list, int index, T? defaultValue = default)
    {
        return list.TryGet(index, out var value)
            ? value
            : defaultValue;
    }

    public static T Sample<T>(this IReadOnlyList<T> list, Random? random = null)
    {
        return (random ?? Random.Shared).Sample(list);
    }

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