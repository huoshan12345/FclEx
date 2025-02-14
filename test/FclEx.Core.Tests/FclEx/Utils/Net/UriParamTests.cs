namespace FclEx.Utils.Net;

public class UriParamTests
{
    [Fact]
    public void Constructor_SetsKeyAndValue()
    {
        var param = new UriParam("key", "value");
        Assert.Equal("key", param.Key);
        Assert.Equal("value", param.Value);
    }

    [Fact]
    public void Constructor_HandlesNullKey()
    {
        var param = new UriParam(null, "value");
        Assert.Equal("", param.Key);
        Assert.Equal("value", param.Value);
    }

    [Fact]
    public void Constructor_HandlesNullValue()
    {
        var param = new UriParam("key", null);
        Assert.Equal("key", param.Key);
        Assert.Equal("", param.Value);
    }

    [Fact]
    public void Constructor_HandlesNullKeyAndValue()
    {
        var param = new UriParam(null, null);
        Assert.Equal("", param.Key);
        Assert.Equal("", param.Value);
    }


    [Fact]
    public void Render_AppendsKeyAndValueToBuilder()
    {
        var builder = new StringBuilder();
        var param = new UriParam("key", "value");
        param.Render(builder);
        Assert.Equal("key=value", builder.ToString());
    }

    [Fact]
    public void Render_HandlesEmptyKey()
    {
        var builder = new StringBuilder();
        var param = new UriParam("", "value");
        param.Render(builder);
        Assert.Equal("value", builder.ToString());

        var col = HttpUtility.ParseQueryString("value");
        Assert.Equal("value", col[null]);
        Assert.Equal("value", col.ToString());
    }

    [Fact]
    public void Render_HandlesEmptyValue()
    {
        var builder = new StringBuilder();
        var param = new UriParam("key", "");
        param.Render(builder);
        Assert.Equal("key=", builder.ToString());


        var col = HttpUtility.ParseQueryString("key=");
        Assert.Equal("", col["key"]);
        Assert.Equal("key=", col.ToString());
    }

    [Fact]
    public void Render_HandlesEmptyKeyAndValue()
    {
        var builder = new StringBuilder();
        var param = new UriParam("", "");
        param.Render(builder);
        Assert.Equal("", builder.ToString());
    }

    [Fact]
    public void Render_EncodesKeyAndValue()
    {
        var builder = new StringBuilder();
        var param = new UriParam("key with spaces", "value with spaces");
        param.Render(builder);
        Assert.Equal("key+with+spaces=value+with+spaces", builder.ToString()); // Note: + is used for space encoding
    }

    [Fact]
    public void ToString_ReturnsRenderedString()
    {
        var param = new UriParam("key", "value");
        Assert.Equal("key=value", param.ToString());
    }

    [Fact]
    public void ToKeyValuePair_ReturnsKeyValuePair()
    {
        var param = new UriParam("key", "value");
        var kvp = param.ToKeyValuePair();
        Assert.Equal("key", kvp.Key);
        Assert.Equal("value", kvp.Value);
    }

    [Fact]
    public void FromKeyValuePair_CreatesUriParam()
    {
        var kvp = new KeyValuePair<string, string>("key", "value");
        var param = UriParam.From(kvp);
        Assert.Equal("key", param.Key);
        Assert.Equal("value", param.Value);
    }

    [Fact]
    public void FromTuple_CreatesUriParam()
    {
        var tuple = ("key", "value");
        var param = UriParam.From(tuple);
        Assert.Equal("key", param.Key);
        Assert.Equal("value", param.Value);
    }

    [Fact]
    public void ImplicitConversionFromTuple_CreatesUriParam()
    {
        UriParam param = ("key", "value");
        Assert.Equal("key", param.Key);
        Assert.Equal("value", param.Value);
    }

    [Fact]
    public void ImplicitConversionFromKeyValuePair_CreatesUriParam()
    {
        KeyValuePair<string, string> kvp = new("key", "value");
        UriParam param = kvp;
        Assert.Equal("key", param.Key);
        Assert.Equal("value", param.Value);
    }

    [Fact]
    public void ImplicitConversionToKeyValuePair_CreatesKeyValuePair()
    {
        UriParam param = new("key", "value");
        KeyValuePair<string, string> kvp = param;
        Assert.Equal("key", kvp.Key);
        Assert.Equal("value", kvp.Value);
    }

    [Fact]
    public void Deconstruct_SetsOutParameters()
    {
        var param = new UriParam("key", "value");
        var (key, value) = param;
        Assert.Equal("key", key);
        Assert.Equal("value", value);
    }
}