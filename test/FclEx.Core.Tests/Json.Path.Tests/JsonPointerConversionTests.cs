namespace Json.Path.Tests;

public class JsonPointerTests
{
    [Fact]
    public void Name()
    {
        var path = JsonPath.Parse("$['foo']");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/foo", asPointer);
    }

    [Fact]
    public void Name_Shorthand()
    {
        var path = JsonPath.Parse("$.foo");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/foo", asPointer);
    }

    [Fact]
    public void Index()
    {
        var path = JsonPath.Parse("$[1]");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/1", asPointer);
    }

    [Fact]
    public void MultipleSegments()
    {
        var path = JsonPath.Parse("$[1].foo");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/1/foo", asPointer);
    }

    [Fact]
    public void NameWithTilde()
    {
        var path = JsonPath.Parse("$['~foo']");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/~0foo", asPointer);
    }

    [Fact]
    public void NameWithSlash()
    {
        var path = JsonPath.Parse("$['/foo']");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/~1foo", asPointer);
    }

    [Fact]
    public void NameWithSurrogatePair()
    {
        var path = JsonPath.Parse("$['\\uD834\\uDD1E']");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/𝄞", asPointer);
    }

    [Fact]
    public void Slice()
    {
        var path = JsonPath.Parse("$[1:2]");

        Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
    }

    [Fact]
    public void RecursiveDescent()
    {
        var path = JsonPath.Parse("$..foo");

        Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
    }

    [Fact]
    public void Wildcard()
    {
        var path = JsonPath.Parse("$[*]");

        Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
    }

    [Fact]
    public void Wildcard_Shorthand()
    {
        var path = JsonPath.Parse("$.*");

        Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
    }

    [Fact]
    public void MultipleSelectors()
    {
        var path = JsonPath.Parse("$[1,'foo']");

        Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
    }

    [Fact]
    public void MultipleSelectorsButSameValue_IndexFirst()
    {
        var path = JsonPath.Parse("$[1,'1']");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/1", asPointer);
    }

    [Fact]
    public void MultipleSelectorsButSameValue_NameFirst()
    {
        var path = JsonPath.Parse("$['1',1]");

        var asPointer = path.AsJsonPointer();

        Assert.Equal("/1", asPointer);
    }
}
