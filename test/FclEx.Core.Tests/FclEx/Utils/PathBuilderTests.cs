namespace FclEx.Utils;

public class PathBuilderTests
{
    [Fact]
    public void AddAndBuild_CombinePartsInOrder()
    {
        var builder = new PathBuilder("root");

        var result = builder.Add("child").Add(["grandchild", "file.txt"]).Build();

        Assert.Equal(Path.Combine("root", "child", "grandchild", "file.txt"), result);
        Assert.Equal(result, builder.ToString());
    }

    [Fact]
    public void Build_WithNoParts_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, new PathBuilder().Build());
    }

    [Fact]
    public void Add_RejectsNullOrEmptyParts()
    {
        var builder = new PathBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Add((string)null!));
        Assert.Throws<ArgumentException>(() => builder.Add(string.Empty));
        Assert.Throws<ArgumentNullException>(() => builder.Add((IEnumerable<string>)null!));
    }

    [Fact]
    public void Add_WithRootedPart_UsesPathCombineSemantics()
    {
        var rootedPath = Path.GetFullPath("rooted");

        var result = new PathBuilder("base").Add(rootedPath).Build();

        Assert.Equal(Path.Combine("base", rootedPath), result);
    }

    [Fact]
    public void Add_WhenLaterPartIsInvalid_KeepsEarlierParts()
    {
        var builder = new PathBuilder("root");

        Assert.Throws<ArgumentException>(() => builder.Add(["child", string.Empty]));

        Assert.Equal(Path.Combine("root", "child"), builder.Build());
    }
}
