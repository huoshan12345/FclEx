namespace FclEx.Extensions.JsonExtensions;

public class IsPossibleJsonTests
{
    private static void Test<T>(T input)
    {
        var json = input.ToJson(new JsonOptions { Indented = true });
        Assert.True(json.IsPossibleJson());
    }

    [Fact]
    public void Null_Test()
    {
        Test((object?)null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Bool_Test(bool input)
    {
        Test(input);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("0")]
    [InlineData("-123")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  \t\n  ")]
    public void String_Test(string input)
    {
        Test(input);
    }

    [Theory]
    [InlineData(123)]
    [InlineData(0)]
    [InlineData(-123)]
    public void Int_Test(int input)
    {
        Test(input);
    }

    [Theory]
    [InlineData(123.123)]
    [InlineData(0.123123)]
    [InlineData(-0.123123)]
    [InlineData(-123.123123)]
    public void Float_Test(double input)
    {
        Test(input);
    }

    [Theory]
    [InlineData("2000-11-01")]
    [InlineData("2000-11-01 01:11:11")]
    public void DateTime_Test(string input)
    {
        var dt = DateTime.Parse(input);
        Test(dt);
    }

    [Fact]
    public void Array_Test()
    {
        Test(new[] { 1, 2, 4 });
    }

    [Fact]
    public void Object_Test()
    {
        Test(new
        {
            Name = "Jim",
            Age = 11
        });
    }
}