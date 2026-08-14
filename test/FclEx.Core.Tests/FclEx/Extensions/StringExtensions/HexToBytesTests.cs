// ReSharper disable ConvertToConstant.Local
namespace FclEx.Extensions.StringExtensions;

public class HexToBytesTests
{
    [Fact]
    public void HexToBytes_ValidHexString_ShouldConvertCorrectly()
    {
        var hex = "4A6F686E";
        var expected = "John"u8.ToArray();
        var result = hex.HexToBytes();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HexToBytes_EmptyString_ShouldReturnEmptyArray()
    {
        var hex = string.Empty;
        var expected = Array.Empty<byte>();
        var result = hex.HexToBytes();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HexToBytes_OddLengthString_ShouldThrowArgumentException()
    {
        var hex = "4A6"; // Odd number of characters
        var exception = Assert.Throws<ArgumentException>(() => hex.HexToBytes());
        Assert.Equal("The binary key cannot have an odd number of digits.", exception.Message);
    }

    [Fact]
    public void HexToBytes_UppercaseAndLowercase_ShouldConvertCorrectly()
    {
        var hex = "aBcDeF";
        byte[] expected = [0xAB, 0xCD, 0xEF];
        var result = hex.HexToBytes();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HexToBytes_ContainsInvalidCharacters_ShouldThrowArgumentException()
    {
        var hex = "4A6G"; // Contains 'G', which is invalid
        var exception = Assert.Throws<ArgumentException>(() => hex.HexToBytes());
        Assert.StartsWith("'G' is not a valid hexadecimal character.", exception.Message);
    }
}
