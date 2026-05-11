using System.Net.NetworkInformation;

namespace FclEx.Extensions;

public class PhysicalAddressExtensionsTests
{
    [Fact]
    public void AddressBytes_ShouldReturnUnderlyingBytes()
    {
        var address = PhysicalAddress.Parse("001122AABBCC");
        var result = address.AddressBytes();
        Assert.Equal([0x00, 0x11, 0x22, 0xAA, 0xBB, 0xCC], result);
    }

    [Theory]
    [InlineData("001122AABBCC", "00:11:22:AA:BB:CC")]
    [InlineData("FFFFFFFFFFFF", "FF:FF:FF:FF:FF:FF")]
    public void ToFormattedString_ShouldUseUpperCaseByDefault(string input, string expected)
    {
        var address = PhysicalAddress.Parse(input);
        var result = address.ToFormattedString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("001122AABBCC", "00-11-22-aa-bb-cc")]
    [InlineData("ABCDEF123456", "ab-cd-ef-12-34-56")]
    public void ToFormattedString_ShouldSupportCustomSeparatorAndLowerCase(
        string input,
        string expected)
    {
        var address = PhysicalAddress.Parse(input);
        var result = address.ToFormattedString("-", lowerCase: true); 
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToFormattedString_ShouldSupportEmptySeparator()
    {
        var address = PhysicalAddress.Parse("001122AABBCC");
        var result = address.ToFormattedString(string.Empty);
        Assert.Equal("001122AABBCC", result);
    }
}
