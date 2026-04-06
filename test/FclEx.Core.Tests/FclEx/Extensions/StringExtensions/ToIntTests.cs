namespace FclEx.Extensions.StringExtensions;

public class ToIntTests
{
    public static TheoryData<string, int> ToIntCases { get; } = Enumerable.Range(-10, 10)
        .Select(m => m * 91 + 3)
        .Select(m => (m.ToString(), m))
        .ToTheoryData();

    [Theory]
    [MemberData(nameof(ToIntCases))]
    public void Test(string str, int expect)
    {
        var i = str.ToInt(defaultValue: int.MinValue);
        Assert.Equal(expect, i);
    }
}