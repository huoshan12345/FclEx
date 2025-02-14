namespace FclEx.Utils;

public class NameIdentifierTests
{
    public sealed record MyNameIdentifier(string Name) : NameIdentifier<MyNameIdentifier>(Name), INameIdentifier<MyNameIdentifier>
    {
        public static MyNameIdentifier Create(string name) => new(name);
    }

    [Fact]
    public void Create_ReturnsNewInstance()
    {
        var identifier = MyNameIdentifier.Create("TestName");
        Assert.Equal("TestName", identifier.Name);
    }

    [Fact]
    public void GetOrCreate_ReturnsCachedInstance()
    {
        var identifier1 = MyNameIdentifier.GetOrCreate("TestName");
        var identifier2 = MyNameIdentifier.GetOrCreate("TestName");
        Assert.Same(identifier1, identifier2); // Should be the same instance from the cache
    }

    [Fact]
    public void GetOrCreate_CreatesNewInstance_WhenCacheDisabled()
    {
        var identifier1 = MyNameIdentifier.GetOrCreate("TestName", useCache: false);
        var identifier2 = MyNameIdentifier.GetOrCreate("TestName", useCache: false);
        Assert.NotSame(identifier1, identifier2); // Should be different instances
    }

    [Fact]
    public void ClearCache_RemovesCachedInstances()
    {
        var identifier1 = MyNameIdentifier.GetOrCreate("TestName");
        MyNameIdentifier.ClearCache();
        var identifier2 = MyNameIdentifier.GetOrCreate("TestName");
        Assert.NotSame(identifier1, identifier2); // Should be a new instance after clearing the cache
    }


    [Fact]
    public void ToString_ReturnsName()
    {
        var identifier = MyNameIdentifier.Create("TestName");
        Assert.Equal("TestName", identifier.ToString());
    }

    [Fact]
    public void GetHashCode_ReturnsHashCodeOfName()
    {
        var identifier = MyNameIdentifier.Create("TestName");
        Assert.Equal("TestName".GetHashCode(), identifier.GetHashCode());
    }

    [Fact]
    public void CompareTo_ReturnsCorrectComparison()
    {
        var identifier1 = MyNameIdentifier.Create("Alpha");
        var identifier2 = MyNameIdentifier.Create("Beta");
        var identifier3 = MyNameIdentifier.Create("Alpha");

        Assert.Equal(-1, identifier1.CompareTo(identifier2));
        Assert.Equal(1, identifier2.CompareTo(identifier1));
        Assert.Equal(0, identifier1.CompareTo(identifier3));
        Assert.Equal(1, identifier1.CompareTo(null)); // Test null comparison.
    }

    [Fact]
    public void CompareTo_HandlesSameInstance()
    {
        var identifier = MyNameIdentifier.Create("Test");
        Assert.Equal(0, identifier.CompareTo(identifier)); // Comparing to itself
    }


    [Fact]
    public void ImplementsINameIdentifier()
    {
        // This test mostly ensures the code compiles correctly, but it's good to have.
        INameIdentifier<MyNameIdentifier> identifier = MyNameIdentifier.Create("Test");
        Assert.Equal("Test", identifier.Name);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameName()
    {
        var id1 = MyNameIdentifier.Create("Test");
        var id2 = MyNameIdentifier.Create("Test");
        Assert.True(id1.Equals(id2));
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentNames()
    {
        var id1 = MyNameIdentifier.Create("Test1");
        var id2 = MyNameIdentifier.Create("Test2");
        Assert.False(id1.Equals(id2));
    }
}