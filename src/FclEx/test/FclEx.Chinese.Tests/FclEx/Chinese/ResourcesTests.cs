namespace FclEx.Chinese;

public class ResourcesTests(ITestOutputHelper output)
{
    [Fact]
    public void STCharacters_Test()
    {
        Characters_Test(ChineseConverter.STCharacters);
    }

    [Fact]
    public void TSCharacters_Test()
    {
        Characters_Test(ChineseConverter.TSCharacters);
    }

    private void Characters_Test(Lazy<Dictionary<string, string>> lazy)
    {
        var dic = lazy.Value;
        Assert.NotEmpty(dic);

        foreach (var (key, value) in dic)
        {
            output.WriteLine($"{key} {value}");
        }
    }
}