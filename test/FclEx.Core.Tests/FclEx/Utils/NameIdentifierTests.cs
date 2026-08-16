#if NET6_0_OR_GREATER
namespace FclEx.Utils;

public class NameIdentifierTests
{
    public sealed record MyNameIdentifier(string Name) : NameIdentifier<MyNameIdentifier>(Name), INameIdentifier<MyNameIdentifier>
    {
        public static MyNameIdentifier Create(string name) => new(name);
    }

    public sealed record NormalizingNameIdentifier(string Name) : NameIdentifier<NormalizingNameIdentifier>(Name), INameIdentifier<NormalizingNameIdentifier>
    {
        public static NormalizingNameIdentifier Create(string name) => new(name.Trim());
    }

    [Fact]
    public void Create_ReturnsNewInstance()
    {
        const string name = nameof(Create_ReturnsNewInstance);
        var identifier = MyNameIdentifier.Create(name);
        Assert.Equal(name, identifier.Name);
    }

    [Fact]
    public void GetOrCreate_ReturnsCachedInstance()
    {
        const string name = nameof(Create_ReturnsNewInstance);
        var identifier1 = MyNameIdentifier.GetOrCreate(name);
        var identifier2 = MyNameIdentifier.GetOrCreate(name);
        Assert.Same(identifier1, identifier2); // Should be the same instance from the cache
    }

    [Fact]
    public void GetOrCreate_CreatesNewInstance_WhenCacheDisabled()
    {
        const string name = nameof(GetOrCreate_CreatesNewInstance_WhenCacheDisabled);
        var identifier1 = MyNameIdentifier.GetOrCreate(name, useCache: false);
        var identifier2 = MyNameIdentifier.GetOrCreate(name, useCache: false);
        Assert.NotSame(identifier1, identifier2); // Should be different instances
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetOrCreate_FactoryThatChangesName_ShouldThrow(bool useCache)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => NormalizingNameIdentifier.GetOrCreate(" name ", useCache));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void ClearCache_RemovesCachedInstances()
    {
        const string name = nameof(ClearCache_RemovesCachedInstances);
        var identifier1 = MyNameIdentifier.GetOrCreate(name);
        MyNameIdentifier.ClearCache();
        var identifier2 = MyNameIdentifier.GetOrCreate(name);
        Assert.NotSame(identifier1, identifier2); // Should be a new instance after clearing the cache
    }


    [Fact]
    public void ToString_ReturnsName()
    {
        const string name = nameof(ToString_ReturnsName);
        var identifier = MyNameIdentifier.Create(name);
        Assert.Equal(name, identifier.ToString());
    }

    [Fact]
    public void GetHashCode_ReturnsHashCodeOfName()
    {
        const string name = nameof(GetHashCode_ReturnsHashCodeOfName);
        var identifier = MyNameIdentifier.Create(name);
        Assert.Equal(name.GetHashCode(), identifier.GetHashCode());
    }

    [Fact]
    public void CompareTo_ReturnsCorrectComparison()
    {
        const string name = nameof(CompareTo_ReturnsCorrectComparison);
        var identifier1 = MyNameIdentifier.Create(name + 1);
        var identifier2 = MyNameIdentifier.Create(name + 2);
        var identifier3 = MyNameIdentifier.Create(name + 1);

        Assert.Equal(-1, identifier1.CompareTo(identifier2));
        Assert.Equal(1, identifier2.CompareTo(identifier1));
        Assert.Equal(0, identifier1.CompareTo(identifier3));
        Assert.Equal(1, identifier1.CompareTo(null)); // Test null comparison.
    }

    [Fact]
    public void CompareTo_HandlesSameInstance()
    {
        var identifier = MyNameIdentifier.Create(nameof(CompareTo_HandlesSameInstance));
        Assert.Equal(0, identifier.CompareTo(identifier)); // Comparing to itself
    }


    [Fact]
    public void ImplementsINameIdentifier()
    {
        const string name = nameof(ImplementsINameIdentifier);
        // This test mostly ensures the code compiles correctly, but it's good to have.
        INameIdentifier<MyNameIdentifier> identifier = MyNameIdentifier.Create(name);
        Assert.Equal(name, identifier.Name);
    }

    [Fact]
    public void Equals_ReturnsTrueForSameName()
    {
        const string name = nameof(Equals_ReturnsTrueForSameName);
        var id1 = MyNameIdentifier.Create(name);
        var id2 = MyNameIdentifier.Create(name);
        Assert.True(id1.Equals(id2));
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentNames()
    {
        const string name = nameof(Equals_ReturnsFalseForDifferentNames);
        var id1 = MyNameIdentifier.Create(name + 1);
        var id2 = MyNameIdentifier.Create(name + 2);
        Assert.False(id1.Equals(id2));
    }
}
#endif
