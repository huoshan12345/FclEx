namespace FclEx.Extensions;

public class IntPtrExtensionsTests
{
    [Fact]
    public void AbsDiff_ReturnsUnsignedDistance()
    {
        Assert.Equal((nuint)7, new IntPtr(3).AbsDiff(new IntPtr(10)));
        Assert.Equal((nuint)7, new IntPtr(10).AbsDiff(new IntPtr(3)));
    }

    [Fact]
    public void AbsDiff_RepresentsFullAddressRange()
    {
        var expected = unchecked((nuint)(nint)(-1));

        Assert.Equal(expected, new IntPtr(-1).AbsDiff(IntPtr.Zero));
    }
}
