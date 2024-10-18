using System.Web;

namespace FclEx.Http.Utils;

public class UriCreatorTests
{
    public static readonly string[] RelativeUris =
    {
        "forum.php",
        "forum.php?mod=viewthread&tid=6762066&pid=138135937&page=1&extra=1",
        "forum.php?mod=viewthread&tid=6762066&pid=138135937&page=1&extra=1#pid138135937",
        "forum.php?mod=viewthread&tid=6762066&pid=138135937&page=1&extra=1&extra=2#pid138135937",
        "forum.php#pid138135937",
    };

    public static readonly IEnumerable<object[]> RelativeUriCases = RelativeUris.Select(m => new object[] { m });
    public static readonly IEnumerable<object[]> AbsoluteUriCases = RelativeUris.Select(m => new object[] { "http://localhost/" + m });

    private static void TestExtra(Uri uri, UriCreator uriCreator)
    {
        Assert.Equal(uri.Fragment, uriCreator.Fragment);
        var map = HttpUtility.ParseQueryString(uri.Query);
        foreach (var key in map.AllKeys)
        {
            var values = map.GetValues(key);
            var actual = uriCreator.Query.GetValues(key);
            Assert.NotNull(actual);
            foreach (var value in values!)
            {
                Assert.Contains(value, actual!);
            }
        }
    }

    [Theory]
    [MemberData(nameof(RelativeUriCases))]
    public void Relative_Test(string str)
    {
        var uri = new Uri(new Uri("http://localhost"), str);
        var uriCreator = new UriCreator(str);

        Assert.Equal(str, uriCreator.Build().ToString());
        Assert.Equal(uri.AbsolutePath.TrimStart('/'), uriCreator.Path);
        TestExtra(uri, uriCreator);
    }

    [Theory]
    [MemberData(nameof(AbsoluteUriCases))]
    public void Absolute_Test(string str)
    {
        var uri = new Uri(str);
        var uriCreator = new UriCreator(str);
            
        Assert.Equal(str, uriCreator.Build().ToString());
        Assert.Equal(uri.AbsolutePath, uriCreator.Path);
        TestExtra(uri, uriCreator);
    }
}