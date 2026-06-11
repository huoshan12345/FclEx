namespace FclEx.Http.Core;

public class HttpHeadersTests
{
    [Fact]
    public void Add_WhenKeyIsNullOrEmpty_IgnoresHeader()
    {
        var headers = new HttpHeaders()
            .Add(null, "null-key")
            .Add("", "empty-key")
            .Add("X-Test", "value");

        Assert.Single(headers);
        Assert.Equal("value", headers.Get("X-Test"));
    }

    [Fact]
    public void Set_WhenValueIsNull_RemovesHeader()
    {
        var headers = new HttpHeaders()
            .Add("X-Test", "value");

        var result = headers.Set("X-Test", null);

        Assert.Same(headers, result);
        Assert.False(headers.ContainsKey("X-Test"));
    }

    [Fact]
    public void Set_WhenKeyIsNullOrEmpty_IgnoresHeader()
    {
        var headers = new HttpHeaders()
            .Set(null, "null-key")
            .Set("", "empty-key")
            .Set("X-Test", "value");

        Assert.Single(headers);
        Assert.Equal("value", headers.Get("X-Test"));
    }

    [Fact]
    public void Set_ReplacesExistingHeaderCaseInsensitively()
    {
        var headers = new HttpHeaders()
            .Add("X-Test", "one");

        headers.Set("x-test", "two");

        Assert.Single(headers);
        Assert.Equal("two", headers.Get("X-TEST"));
    }

    [Fact]
    public void Headers_AreCaseInsensitive()
    {
        var headers = new HttpHeaders()
            .Add("X-Test", "one")
            .Add("x-test", "two");

        Assert.True(headers.ContainsKey("X-TEST"));
        Assert.Equal(["one", "two"], headers.GetValues("X-TEST"));
    }

    [Fact]
    public void Render_WritesEachHeaderAsHttpHeaderLine()
    {
        var headers = new HttpHeaders()
            .Add("Accept", "application/json")
            .Add("X-Test", "value");
        var builder = new StringBuilder();

        headers.Render(builder);

        Assert.Equal("Accept: application/json\r\nX-Test: value\r\n", builder.ToString());
    }

    [Fact]
    public void Parse_CreatesHeadersFromQueryStyleString()
    {
        var headers = HttpHeaders.Parse("Accept=application%2Fjson&X-Test=value");

        Assert.Equal("application/json", headers.Get("Accept"));
        Assert.Equal("value", headers.Get("X-Test"));
    }

    [Fact]
    public void Parse_WhenInputIsNull_ReturnsEmptyHeaders()
    {
        var headers = HttpHeaders.Parse(null);

        Assert.Empty(headers);
    }

    [Fact]
    public void From_CreatesHeadersFromPairs()
    {
        var headers = HttpHeaders.From(
        [
            KeyValuePair.Create("Accept", "application/json"),
            KeyValuePair.Create("X-Test", "value"),
        ]);

        Assert.Equal("application/json", headers.Get("Accept"));
        Assert.Equal("value", headers.Get("X-Test"));
    }

    [Fact]
    public void From_WithSingleGenericValue_ConvertsValueToHeaderString()
    {
        var headers = HttpHeaders.From("X-Retry", 3);

        Assert.Equal("3", headers.Get("X-Retry"));
    }
}
