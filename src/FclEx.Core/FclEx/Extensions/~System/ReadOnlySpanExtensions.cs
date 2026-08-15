namespace FclEx.Extensions;

public static class ReadOnlySpanExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static bool StartsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
    {
        return span.IsEmpty == false
               && EqualityComparer<T>.Default.Equals(span[0], value);
    }

    [MethodImpl(AggressiveInlining)]
    public static bool EndsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
    {
        return span.IsEmpty == false
               && EqualityComparer<T>.Default.Equals(span[^1], value);
    }

    [MethodImpl(AggressiveInlining)]
    public static string GetString(this ReadOnlySpan<byte> span, Encoding? encoding = null)
    {
        return (encoding ?? Encoding.UTF8).GetString(span);
    }

#if NET6_0_OR_GREATER
    public static string ToBase64(this ReadOnlySpan<byte> span) => Convert.ToBase64String(span);
#endif

    /// <summary>
    /// Reads an unmanaged value from the beginning of the span using the managed layout of <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="StructLayoutAttribute"/> and fixed buffers when the bytes originate from a native structure.
    /// No byte-order conversion or interop marshaling is performed. Bytes after the value are ignored.
    /// </remarks>
    public static T ReadStruct<T>(this ReadOnlySpan<byte> span) where T : unmanaged
    {
        return MemoryMarshal.Read<T>(span);
    }

    /// <summary>
    /// Reads consecutive unmanaged values from the span.
    /// </summary>
    /// <exception cref="ArgumentException">The span length is not an exact multiple of the structure size.</exception>
    public static T[] ReadStructArray<T>(this ReadOnlySpan<byte> span) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        if (span.Length % size != 0)
            throw new ArgumentException("The span length must be an exact multiple of the structure size.", nameof(span));

        var count = span.Length / size;
        var result = new T[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = MemoryMarshal.Read<T>(span.Slice(i * size, size));
        }
        return result;
    }

    public static byte[] ToBytes(this ReadOnlySpan<bool> bits)
    {
        var count = bits.Length;
        var numBytes = count / 8;
        if (count % 8 != 0)
            numBytes++;

        var bytes = new byte[numBytes];
        int byteIndex = 0, bitIndex = 0;

        foreach (var bit in bits)
        {
            if (bit) bytes[byteIndex] |= (byte)(1 << bitIndex);
            ++bitIndex;

            if (bitIndex == 8)
            {
                bitIndex = 0;
                ++byteIndex;
            }

        }
        return bytes;
    }

    /// <summary>
    /// Casts a ReadOnlySpan of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
    /// These types may not contain pointers or references. This is checked at runtime in order to preserve type safety.
    /// </summary>
    /// <remarks>
    /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
    /// </remarks>
    /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> contains pointers.
    /// </exception>
    [MethodImpl(AggressiveInlining)]
    public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(this ReadOnlySpan<TFrom> span)
        where TFrom : struct
        where TTo : struct
    {
        return MemoryMarshal.Cast<TFrom, TTo>(span);
    }

    /// <summary>
    /// Casts a ReadOnlySpan of one primitive type <typeparamref name="T"/> to ReadOnlySpan of bytes.
    /// That type may not contain pointers or references. This is checked at runtime in order to preserve type safety.
    /// </summary>
    /// <param name="span">The source slice, of type <typeparamref name="T"/>.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <typeparamref name="T"/> contains pointers.
    /// </exception>
    /// <exception cref="System.OverflowException">
    /// Thrown if the Length property of the new Span would exceed int.MaxValue.
    /// </exception>
    [MethodImpl(AggressiveInlining)]
    public static ReadOnlySpan<byte> AsBytes<T>(this ReadOnlySpan<T> span) where T : struct
    {
        return MemoryMarshal.AsBytes(span);
    }

    public static TCollection ToCollection<T, TCollection>(this ReadOnlySpan<T> span, Func<TCollection> factory) where TCollection : ICollection<T>
    {
        var col = factory();
        foreach (var item in span)
        {
            col.Add(item);
        }
        return col;
    }

    [MethodImpl(AggressiveInlining)]
    public static TCollection ToCollection<T, TCollection>(this ReadOnlySpan<T> span) where TCollection : ICollection<T>, new()
    {
        return span.ToCollection(() => new TCollection());
    }

    [MethodImpl(AggressiveInlining)]
    public static HashSet<T> ToHashSet<T>(this ReadOnlySpan<T> span)
    {
        return span.ToCollection<T, HashSet<T>>();
    }

    [MethodImpl(AggressiveInlining)]
    public static List<T> ToList<T>(this ReadOnlySpan<T> span)
    {
        return span.ToCollection<T, List<T>>();
    }

    public static int ComputeHashCode<T>(this ReadOnlySpan<T> span)
    {
        var code = 0;
        foreach (var value in span)
        {
            code = HashCode.Combine(value, code);
        }
        return code;
    }

    public static int ComputeHashCode(this ReadOnlySpan<byte> span)
    {
        const int sizeOfInt = sizeof(int);
        var count = span.Length / sizeOfInt;
        var remaining = span.Length % sizeOfInt;

        var code = 0;
        for (var i = 0; i < count; i++)
        {
            var intSpan = span.Slice(i * sizeOfInt, sizeOfInt);
            var intVal = intSpan.ToInt32();
            code = HashCode.Combine(intVal, code);
        }

        for (var i = 1; i <= remaining; i++)
        {
            code = HashCode.Combine(span[^i], code);
        }

        return code;
    }

    public static SplitEnumerator EnumerateSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separators, StringSplitOptions options)
        => new(span, separators, options.ToSplitOptions());

    public static SplitEnumerator EnumerateSplit(this ReadOnlySpan<char> span, ReadOnlySpan<char> separators, SplitOptions options = SplitOptions.TrimAndRemoveEmpty)
        => new(span, separators, options);

    public ref struct SplitEnumerator
    {
        private readonly ReadOnlySpan<char> _separators;
        private readonly SplitOptions _options;
        private ReadOnlySpan<char> _remaining;
        private ReadOnlySpan<char> _current;
        private bool _hasResult;

        public SplitEnumerator(
            ReadOnlySpan<char> span,
            ReadOnlySpan<char> separators,
            SplitOptions options)
        {
            _remaining = span;
            _separators = separators;
            _options = options;
            _current = default;
            _hasResult = true;
        }

        public readonly SplitEnumerator GetEnumerator() => this;

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public readonly ReadOnlySpan<char> Current => _current;

        public bool MoveNext()
        {
            while (true)
            {
                if (_hasResult == false)
                    return false;

                var idx = _remaining.IndexOfAny(_separators);

                ReadOnlySpan<char> slice;

                if (idx < 0)
                {
                    slice = _remaining;
                    _remaining = default;
                    _hasResult = false;
                }
                else
                {
                    slice = _remaining[..idx];
                    _remaining = _remaining[(idx + 1)..];
                }

                // TrimEntries
                if ((_options & SplitOptions.TrimEntries) != 0)
                {
                    slice = slice.Trim();
                }

                // RemoveEmptyEntries
                if ((_options & SplitOptions.RemoveEmptyEntries) != 0 && slice.IsEmpty)
                {
                    continue;
                }

                _current = slice;
                return true;
            }
        }
    }
}
