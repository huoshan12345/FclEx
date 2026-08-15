namespace FclEx.Extensions;

public class BitsToIntTests
{
    [Fact]
    public void BitsToInt_ShouldTreatTheFirstItemAsTheLeastSignificantBit()
    {
        Assert.Equal(0, Array.Empty<bool>().BitsToInt());
        Assert.Equal(13, new[] { true, false, true, true }.BitsToInt());
    }

    [Fact]
    public void BitsToInt_ShouldSupportTheSignBit()
    {
        var bits = new bool[32];
        bits[31] = true;

        Assert.Equal(int.MinValue, bits.BitsToInt());
    }

    [Fact]
    public void BitsToInt_ShouldRejectMoreThan32Bits()
    {
        Assert.Throws<ArgumentException>(() => new bool[33].BitsToInt());
    }

    [Fact]
    public void BitsToInt_ShouldRejectNull()
    {
        IEnumerable<bool>? bits = null;

        Assert.Throws<ArgumentNullException>(() => bits!.BitsToInt());
    }
}
