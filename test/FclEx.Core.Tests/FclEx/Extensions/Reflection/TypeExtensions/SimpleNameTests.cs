namespace FclEx.Extensions.Reflection.TypeExtensions;

public class SimpleNameTests
{
    public static readonly TheoryData<Type, string> Cases = new()
    {
        (typeof(int), nameof(Int32)),
        (typeof(Dictionary<string, int>), "Dictionary"),
        (typeof(Dictionary<List<string>, HashSet<int>>), "Dictionary"),
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Test(Type type, string expectedName)
    {
        var name = type.SimpleName();
        Assert.Equal(expectedName, name);
    }
}