namespace FclEx.Extensions;

public static class StringInfoExtensions
{
#if NET9_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Indexes")]
    internal static extern int[]? GetStringInfoIndexes(StringInfo si);

    internal readonly ref struct UnsafeReadOnlySpan<T>(ReadOnlySpan<T> span)
    {
        private readonly ReadOnlySpan<T> _span = span;
        public ref T this[int index] => ref Unsafe.Add(ref MemoryMarshal.GetReference(_span), index);

        public UnsafeReadOnlySpan<T> Slice(int start) => Slice(start, _span.Length - start);
        public UnsafeReadOnlySpan<T> Slice(int start, int length)
            => new(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(_span), start), length));

        public int Length => _span.Length;
        public static implicit operator ReadOnlySpan<T>(UnsafeReadOnlySpan<T> span) => span._span;
        public static implicit operator UnsafeReadOnlySpan<T>(ReadOnlySpan<T> span) => new(span);
    }

    public ref struct StringInfoSpanEnumerator(StringInfo si)
    {
        private readonly UnsafeReadOnlySpan<char> _str = si.String;
        private readonly ReadOnlySpan<int> _indexes = GetStringInfoIndexes(si) ?? [];
        private int _index = -1;

        public readonly ReadOnlySpan<char> Current =>
            _index < _indexes.Length - 1
                ? _str[_indexes[_index].._indexes[_index + 1]]
                : _str[_indexes[_index]..];

        public void Dispose() => Reset();
        public bool MoveNext() => ++_index < _indexes.Length;
        public void Reset() => _index = -1;

        public readonly StringInfoSpanEnumerator GetEnumerator() => this;
    }

    public static StringInfoSpanEnumerator GetTextElementEnumerator(this StringInfo si) => new(si);
    public static StringInfoSpanEnumerator GetTextElementEnumerator(this string si) => new StringInfo(si).GetTextElementEnumerator();
#endif
}
