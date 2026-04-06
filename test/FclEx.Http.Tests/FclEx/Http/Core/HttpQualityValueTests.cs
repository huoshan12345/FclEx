namespace FclEx.Http.Core;

public class HttpQualityValueTests
{
    //  When no "q=" is specified, it defaults to 1
    public static readonly string[] AcceptEncodings =
    [
        "gzip,deflate",
        "deflate,gzip",
        "gzip;q=.5,deflate",
        "gzip;q=0,deflate",
        "deflate;q=0.5,gzip;q=0.5,identity",
        "*",
    ];
    public static readonly string[] PreferOrder = ["gzip", "deflate"];
    public static readonly string?[] ExpectedEncoding =
    [
        "gzip",
        "gzip",
        "deflate",
        "deflate",
        null,
        "gzip",
    ];

    public static readonly TheoryData<string, string?> Cases = AcceptEncodings.Zip(ExpectedEncoding, (a, e) => (a, e)).ToTheoryData();

    [Theory]
    [MemberData(nameof(Cases))]
    public void Test(string acceptEncoding, string? expectedEncoding)
    {
        var encodings = new HttpQualityValueList(acceptEncoding);
        var preferred = encodings.FindPreferred(PreferOrder);
        Assert.Equal(expectedEncoding, preferred.Name);
    }
}