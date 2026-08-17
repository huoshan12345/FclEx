using static FclEx.Extensions.SeparatorLocationOption;

namespace FclEx.Extensions.StringExtensions;

public class PartitionTests
{
    [Theory]
    [InlineData(false, "left::", "::middle::right")]
    [InlineData(true, "left::middle::", "::right")]
    public void Both_Includes_A_MultiCharacter_Separator_In_Both_Parts(
        bool fromRight,
        string expectedLeft,
        string expectedRight)
    {
        var result = "left::middle::right".Partition("::", Both, fromRight);

        Assert.Equal((expectedLeft, expectedRight), result);
    }
}
