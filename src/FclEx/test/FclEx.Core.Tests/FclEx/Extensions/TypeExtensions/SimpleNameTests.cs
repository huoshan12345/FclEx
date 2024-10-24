namespace FclEx.Extensions.TypeExtensions;

public class SimpleNameTests
{
    public static IEnumerable<object[]> Cases { get; } = new(object, string)[]
    {
        (typeof(int), nameof(Int32)),
        (typeof(Dictionary<string, int>), "Dictionary"),
        (typeof(Dictionary<List<string>, HashSet<int>>), "Dictionary"),
    }.Select(m => new object[] { m.Item1, m.Item2 }).ToArray();

    [Theory]
    [MemberData(nameof(Cases))]
    public void Test(Type type, string expectedName)
    {
        var name = type.SimpleName();
        Assert.Equal(expectedName, name);
    }
}