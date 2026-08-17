namespace System.Collections.Generic;

public class BitwiseEqualityComparerTests
{
    [Fact]
    public void Equals_IdenticalRepresentations_ReturnsTrue()
    {
        var value = new CompositeValue(42, 1.25);
        var copy = value;

        Assert.True(BitwiseEqualityComparer<CompositeValue>.Instance.Equals(value, copy));
    }

    [Fact]
    public void Equals_DifferentRepresentations_ReturnsFalse()
    {
        var x = new CompositeValue(42, 1.25);
        var y = new CompositeValue(42, 1.5);

        Assert.False(BitwiseEqualityComparer<CompositeValue>.Instance.Equals(x, y));
    }

    [Fact]
    public void Equals_PositiveAndNegativeZero_ReturnsFalse()
    {
        Assert.Equal(0F, -0F);
        Assert.False(BitwiseEqualityComparer<float>.Instance.Equals(0F, -0F));
    }

    [Fact]
    public void Equals_DifferentPaddingBytes_ReturnsFalse()
    {
        var x = new PaddedValue { Value = 42 };
        var y = x;
        AsBytes(ref y)[^1] = 1;

        Assert.False(BitwiseEqualityComparer<PaddedValue>.Instance.Equals(x, y));
    }

    [Fact]
    public void GetHashCode_IdenticalRepresentations_ReturnsSameHashCode()
    {
        var value = new CompositeValue(42, 1.25);
        var copy = value;
        var comparer = BitwiseEqualityComparer<CompositeValue>.Instance;

        Assert.Equal(comparer.GetHashCode(value), comparer.GetHashCode(copy));
    }

    private static unsafe Span<byte> AsBytes<T>(ref T value) where T : unmanaged
    {
        return new(Unsafe.AsPointer(ref value), sizeof(T));
    }

    private readonly record struct CompositeValue(long Number, double Amount);

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    private struct PaddedValue
    {
        [FieldOffset(0)]
        public int Value;
    }
}
