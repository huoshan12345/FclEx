namespace FclEx.Extensions.Reflection.TypeExtensions;

public class ShortNameTests
{
    public static readonly TheoryData<Type, string> Cases = new()
    {
        (typeof(int), nameof(Int32)),
        (typeof(Dictionary<string, int>), "Dictionary<String, Int32>"),
        (typeof(Dictionary<List<string>, HashSet<int>>), "Dictionary<List<String>, HashSet<Int32>>"),
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Test(Type type, string expectedName)
    {
        var name = type.ShortName();
        Assert.Equal(expectedName, name);
    }

    [Fact]
    public void TestAllType()
    {
        var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(m => m.GetTypes()).ToArray();
        foreach (var type in allTypes)
        {
            var simpleName = type.SimpleName();
            var shortName = type.ShortName();
            if (!type.IsGenericType)
            {
                Assert.Equal(simpleName, shortName);
            }
        }
    }

    [Fact]
    public void GenericParameter_Test()
    {
        var type = typeof(List<>).GetGenericArguments().First();
        Assert.True(type.IsGenericParameter);
        var shortName = type.ShortName();
        Assert.NotNull(shortName);
    }
}