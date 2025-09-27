namespace Xunit;

partial class AssertExTests
{
    [Fact]
    public void BuildExcludeMemberTree_Test()
    {
        var paths = new[]
        {
            "c.a",
            "c.b",
            "a",
            "c",
            "a.b.c",
            "c.b.a.d",
            "b.a.d",
        };

        var expected = new[]
        {
            ("$", false),

            ("a", true),
            ("b", false),
            ("c", true),

            ("b", false),
            ("a", false),
            ("a", true),
            ("b", true),

            ("c", true),
            ("d", true),
            ("a", false),

            ("d", true),
        };

        var tree = AssertEx.BuildExcludeMemberTree(paths);
        var actual = tree.TraversalByLevel().Select(m => (m.Name, m.IsExcluded)).ToArray();

        Assert.Equal(expected.Length, actual.Length);
        foreach (var (e, a) in expected.Zip(actual))
        {
            Assert.Equal(e, a);
        }
    }
}