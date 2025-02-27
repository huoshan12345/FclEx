namespace FclEx.Utils.Net;

public class UriParamsTests
{
    [Fact]
    public void Constructor()
    {
        var uriParams = new UriParams();
        Assert.Empty(uriParams);
    }

    [Fact]
    public void Parse_NullQuery()
    {
        var uriParams = UriParams.Parse(null);
        Assert.Empty(uriParams);
    }

    [Fact]
    public void Parse_WithQueryString()
    {
        var uriParams = UriParams.Parse("key1=value1&key2=value2");
        Assert.Equal(2, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
        Assert.Equal("value2", uriParams["key2"]);
    }

    [Fact]
    public void Parse_WithDuplicateKeys()
    {
        var uriParams = UriParams.Parse("key1=value1&key1=value2");
        Assert.Equal(2, uriParams.Count);
        Assert.Equal("value2", uriParams["key1"]); //Last one wins in indexer
        Assert.Equal(["value1", "value2"], uriParams.GetValues("key1"));
    }

    [Fact]
    public void Constructor_WithKeyValuePairsEnumerable()
    {
        var uriParamsList = new List<KeyValuePair<string, string>> { new("key1", "value1"), new("key2", "value2") };
        var uriParams = UriParams.From(uriParamsList);
        Assert.Equal(2, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
        Assert.Equal("value2", uriParams["key2"]);
    }

    [Fact]
    public void From_WithSingleKeyValuePair()
    {
        var uriParams = UriParams.From("key1", "value1");
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
    }

    [Fact]
    public void ToString_RendersCorrectly()
    {
        var uriParams = UriParams.Parse("key1=value1&key2=value2");
        Assert.Equal("key1=value1&key2=value2", uriParams.ToString());
    }

    [Fact]
    public void Render_AppendsToStringBuilder()
    {
        var sb = new StringBuilder();
        var uriParams = UriParams.Parse("key1=value1&key2=value2");
        uriParams.Render(sb);
        Assert.Equal("key1=value1&key2=value2", sb.ToString());
    }

    [Fact]
    public void Add_AddsParameter()
    {
        var uriParams = new UriParams { { "key1", "value1" } };
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
    }

    [Fact]
    public void Add_HandlesNullValue()
    {
        var uriParams = new UriParams { { "key1", null } };
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("", uriParams["key1"]);
    }

    [Fact]
    public void Add_AddsDuplicateKey()
    {
        var uriParams = UriParams.Parse("key1=value1").Add("key1", "value2");
        Assert.Equal(2, uriParams.Count);
        Assert.Equal(["value1", "value2"], uriParams.GetValues("key1"));
    }

    [Fact]
    public void Set_SetsParameter()
    {
        var uriParams = new UriParams();
        uriParams.Set("key1", "value1");
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
    }

    [Fact]
    public void Set_ReplacesExistingValue()
    {
        var uriParams = UriParams.Parse("key1=oldValue");
        uriParams.Set("key1", "newValue");
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("newValue", uriParams["key1"]);
    }

    [Fact]
    public void Remove_RemovesParameter()
    {
        var uriParams = UriParams.Parse("key1=value1&key2=value2");
        uriParams.Remove("key1");
        Assert.Equal(1, uriParams.Count);
        Assert.Null(uriParams["key1"]);
        Assert.Equal("value2", uriParams["key2"]);
    }

    [Fact]
    public void Get_RetrievesLatestValue()
    {
        var uriParams = UriParams.Parse("key1=value1&key1=value2");
        Assert.Equal("value2", uriParams.Get("key1"));
    }

    [Fact]
    public void Get_ReturnsNullForNonExistingKey()
    {
        var uriParams = new UriParams();
        Assert.Null(uriParams.Get("key1"));
    }

    [Fact]
    public void Indexer_SetsAndGetsValue()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var uriParams = new UriParams();
        uriParams["key1"] = "value1";
        Assert.Equal("value1", uriParams["key1"]);
        uriParams["key1"] = "newValue";
        Assert.Equal("newValue", uriParams["key1"]);
    }

    [Fact]
    public void GetValues_RetrievesAllValues()
    {
        var uriParams = UriParams.Parse("key1=value1&key1=value2");
        var values = uriParams.GetValues("key1");
        Assert.NotNull(values);
        Assert.Equal(2, values.Count);
        Assert.Contains("value1", values);
        Assert.Contains("value2", values);
    }

    [Fact]
    public void GetValues_ReturnsEmptyForNonExistingKey()
    {
        var uriParams = new UriParams();
        var values = uriParams.GetValues("key1");
        Assert.Null(values);
    }

    [Fact]
    public void TryGet_RetrievesValue()
    {
        var uriParams = UriParams.Parse("key1=value1");
        Assert.True(uriParams.TryGet("key1", out var value));
        Assert.Equal("value1", value);
    }

    [Fact]
    public void TryGet_ReturnsFalseForNonExistingKey()
    {
        var uriParams = new UriParams();
        Assert.False(uriParams.TryGet("key1", out var value));
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValues_RetrievesValues()
    {
        var uriParams = UriParams.Parse("key1=value1&key1=value2");
        Assert.True(uriParams.TryGetValues("key1", out var values));
        Assert.Equal(2, values.Count);
        Assert.Contains("value1", values);
        Assert.Contains("value2", values);
    }

    [Fact]
    public void TryGetValues_ReturnsFalseForNonExistingKey()
    {
        var uriParams = new UriParams();
        Assert.False(uriParams.TryGetValues("key1", out var values));
        Assert.Null(values);
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        var uriParams = UriParams.Parse("key1=value1&key2=value2&key1=value3");
        Assert.Equal(3, uriParams.Count);
    }

    [Fact]
    public void Parse_CreatesUriParams()
    {
        var uriParams = UriParams.Parse("key1=value1&key2=value2");
        Assert.Equal(2, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
        Assert.Equal("value2", uriParams["key2"]);
    }

    [Fact]
    public void From_CreatesUriParamsFromKeyValuePairs()
    {
        var kvpList = new List<KeyValuePair<string, string>> { new("key1", "value1"), new("key2", "value2") };
        var uriParams = UriParams.From(kvpList);
        Assert.Equal(2, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
        Assert.Equal("value2", uriParams["key2"]);
    }

    [Fact]
    public void From_CreatesUriParamsFromSingleKeyValuePair()
    {
        var uriParams = UriParams.From("key1", "value1");
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("value1", uriParams["key1"]);
    }

    [Fact]
    public void From_CreatesUriParamsFromSingleKeyValuePair_ObjectValue()
    {
        var uriParams = UriParams.From("key1", 123);
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("123", uriParams["key1"]);
    }


    [Fact]
    public void GetEnumerator_EnumeratesCorrectly()
    {
        var uriParams = UriParams.Parse("key1=value1&key2=value2&key1=value3");
        var enumeratedParams = uriParams.ToList();
        Assert.Equal(3, enumeratedParams.Count);
        Assert.Contains(KeyValuePair.Create("key1", "value1"), enumeratedParams);
        Assert.Contains(KeyValuePair.Create("key2", "value2"), enumeratedParams);
        Assert.Contains(KeyValuePair.Create("key1", "value3"), enumeratedParams);
    }

    [Fact]
    public void Add_HandlesSpecialCharacters()
    {
        var uriParams = new UriParams
        {
            { "key with spaces", "value with spaces" },
            { "key&equals", "value=equals" },
            { "key+plus", "value+plus" },
            { "key/slash", "value/slash" },
            { "key?question", "value?question" },
            { "key#hash", "value#hash" },
            { "key%percent", "value%percent" },
        };

        Assert.Equal("value with spaces", uriParams["key with spaces"]);
        Assert.Equal("value=equals", uriParams["key&equals"]);
        Assert.Equal("value+plus", uriParams["key+plus"]);
        Assert.Equal("value/slash", uriParams["key/slash"]);
        Assert.Equal("value?question", uriParams["key?question"]);
        Assert.Equal("value#hash", uriParams["key#hash"]);
        Assert.Equal("value%percent", uriParams["key%percent"]);

        Assert.Equal("key+with+spaces=value+with+spaces&key%26equals=value%3dequals&key%2bplus=value%2bplus&key%2fslash=value%2fslash&key%3fquestion=value%3fquestion&key%23hash=value%23hash&key%25percent=value%25percent", uriParams.ToString());

    }

    [Fact]
    public void Set_HandlesSpecialCharacters()
    {
        var uriParams = new UriParams();
        uriParams.Set("key with spaces", "value with spaces");
        uriParams.Set("key&equals", "value=equals");
        uriParams.Set("key+plus", "value+plus");
        uriParams.Set("key/slash", "value/slash");
        uriParams.Set("key?question", "value?question");
        uriParams.Set("key#hash", "value#hash");
        uriParams.Set("key%percent", "value%percent");

        Assert.Equal("value with spaces", uriParams["key with spaces"]);
        Assert.Equal("value=equals", uriParams["key&equals"]);
        Assert.Equal("value+plus", uriParams["key+plus"]);
        Assert.Equal("value/slash", uriParams["key/slash"]);
        Assert.Equal("value?question", uriParams["key?question"]);
        Assert.Equal("value#hash", uriParams["key#hash"]);
        Assert.Equal("value%percent", uriParams["key%percent"]);

        Assert.Equal("key+with+spaces=value+with+spaces&key%26equals=value%3dequals&key%2bplus=value%2bplus&key%2fslash=value%2fslash&key%3fquestion=value%3fquestion&key%23hash=value%23hash&key%25percent=value%25percent", uriParams.ToString());
    }


    [Fact]
    public void Add_HandlesNullKey()
    {
        var uriParams = new UriParams { { null, "value" } };
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("value", uriParams[""]); // Empty string key
    }

    [Fact]
    public void Set_HandlesNullKey()
    {
        var uriParams = new UriParams();
        uriParams.Set(null, "value");
        Assert.Equal(1, uriParams.Count);
        Assert.Equal("value", uriParams[""]); // Empty string key
    }

    [Fact]
    public void Remove_HandlesNullKey()
    {
        var uriParams = new UriParams { { null, "value" } };
        uriParams.Remove(null);
        Assert.Empty(uriParams);
    }

    [Fact]
    public void Get_HandlesNullKey()
    {
        var uriParams = new UriParams { { null, "value" } };
        Assert.Equal("value", uriParams.Get(null));
    }

    [Fact]
    public void GetValues_HandlesNullKey()
    {
        var uriParams = new UriParams
        {
            { null, "value1" },
            { null, "value2" },
        };
        var values = uriParams.GetValues(null);
        Assert.NotNull(values);
        Assert.Equal(2, values.Count);
        Assert.Contains("value1", values);
        Assert.Contains("value2", values);
    }

    [Fact]
    public void TryGet_HandlesNullKey()
    {
        var uriParams = new UriParams { { null, "value" } };
        Assert.True(uriParams.TryGet(null, out var value));
        Assert.Equal("value", value);
    }

    [Fact]
    public void TryGetValues_HandlesNullKey()
    {
        var uriParams = new UriParams
        {
            { null, "value1" },
            { null, "value2" },
        };
        Assert.True(uriParams.TryGetValues(null, out var values));
        Assert.Equal(2, values.Count);
        Assert.Contains("value1", values);
        Assert.Contains("value2", values);
    }
}