namespace FclEx.Extensions.ReadOnlySpanExtensions;

public class PackBitsTests
{
    [Fact]
    public void PackBits_UsesLeastSignificantBitFirstOrderAndZeroPadsTheFinalByte()
    {
        bool[] bits = [true, false, false, false, false, false, false, true, true, true];

        var packed = bits.AsSpan().PackBits();

        Assert.Equal([0x81, 0x03], packed);
    }

    [Fact]
    public void UnpackBits_UsesLeastSignificantBitFirstOrder()
    {
        byte[] bytes = [0x81, 0x03];

        var unpacked = bytes.AsSpan().UnpackBits();

        Assert.Equal(
            [true, false, false, false, false, false, false, true, true, true, false, false, false, false, false, false],
            unpacked);
    }

    [Fact]
    public void UnpackBits_AfterPackBits_PreservesTheOriginalPrefix()
    {
        bool[] bits = [true, false, true, false, false, false, false, false, true, false];

        var unpacked = bits.AsSpan().PackBits().AsSpan().UnpackBits();

        Assert.Equal(bits, unpacked.Take(bits.Length));
        Assert.All(unpacked.Skip(bits.Length), Assert.False);
    }
}
