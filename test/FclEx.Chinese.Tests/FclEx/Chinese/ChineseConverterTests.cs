using System.Runtime.InteropServices;
using static FclEx.Chinese.ChineseConversionDirection;

namespace FclEx.Chinese;

public class ChineseConverterTests
{
    [Theory]
    [InlineData("後來", "後来")]
    [InlineData("北京時間", "北京时间")]
    [InlineData("生命不息，奮鬥不止", "生命不息，奋斗不止")]
    public void ConvertByWinApi_ToSimplified_Test(string text, string expected)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            return;

        var converted = ChineseConverter.ConvertByWinApi(text, ToSimplified);
        Assert.Equal(expected, converted);
    }

    [Theory]
    [InlineData("后来", "后來")]
    [InlineData("北京时间", "北京時間")]
    [InlineData("生命不息，奋斗不止", "生命不息，奮斗不止")]
    public void ConvertByWinApi_ToTraditional_Test(string text, string expected)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            return;

        var converted = ChineseConverter.ConvertByWinApi(text, ToTraditional);
        Assert.Equal(expected, converted);
    }

    [Theory]
    [InlineData("後來", "后来")]
    [InlineData("北京時間", "北京时间")]
    [InlineData("生命不息，奮鬥不止", "生命不息，奋斗不止")]
    public void ConvertUsingDictionary_ToSimplified_Test(string text, string expected)
    {
        var converted = ChineseConverter.ConvertUsingDictionary(text, ToSimplified);
        Assert.Equal(expected, converted);
    }

    [Theory]
    [InlineData("后来", "後來")]
    [InlineData("北京时间", "北京時間")]
    [InlineData("生命不息，奋斗不止", "生命不息，奮鬥不止")]
    public void ConvertUsingDictionary_ToTraditional_Test(string text, string expected)
    {
        var converted = ChineseConverter.ConvertUsingDictionary(text, ToTraditional);
        Assert.Equal(expected, converted);
    }

    [Theory]
    [InlineData("後來", "后来")]
    [InlineData("北京時間", "北京时间")]
    [InlineData("生命不息，奮鬥不止", "生命不息，奋斗不止")]
    public void Convert_ToSimplified_Test(string text, string expected)
    {
        var converted = ChineseConverter.Convert(text, ToSimplified);
        Assert.Equal(expected, converted);
    }

    [Theory]
    [InlineData("后来", "後來")]
    [InlineData("北京时间", "北京時間")]
    [InlineData("生命不息，奋斗不止", "生命不息，奮鬥不止")]
    public void Convert_ToTraditional_Test(string text, string expected)
    {
        var converted = ChineseConverter.Convert(text, ToTraditional);
        Assert.Equal(expected, converted);
    }
}