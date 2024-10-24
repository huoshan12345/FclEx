namespace FclEx.Serialization;

public class JsonMemoryBytesSerializerTests
{
    private static void Test<T>(T input, EqualityComparer<T?>? comparer = null)
    {
        var serializer = JsonMemoryBytesSerializer.Instance;
        var bytes = serializer.Serialize(input);
        var output = serializer.Deserialize<T>(bytes);
        if (comparer == null)
            Assert.Equal(input, output);
        else
            Assert.Equal(input, output, comparer);
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
    [InlineData("123")]
    [InlineData("0")]
    [InlineData("-123")]
    [InlineData("")]
    [InlineData("  \t\n  ")]
    public void String_Test(string input)
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
}