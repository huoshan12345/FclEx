namespace System.Text.Json;

public class ObjectJsonConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectJsonConverter());
        return options;
    }

    [Fact]
    public void Deserialize_Int_ShouldReturnInt()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("1", options);

        Assert.IsType<int>(result);
        Assert.Equal(1, result);
    }

    [Fact]
    public void Deserialize_Bool_ShouldReturnBool()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("true", options);

        Assert.IsType<bool>(result);
        Assert.True((bool)result!);
    }

    [Fact]
    public void Deserialize_String_ShouldReturnString()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("\"abc\"", options);

        Assert.IsType<string>(result);
        Assert.Equal("abc", result);
    }

    [Fact]
    public void Deserialize_Null_ShouldReturnNull()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("null", options);

        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_Array_ShouldReturnList()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("[1,2,3]", options);

        var list = Assert.IsType<List<object>>(result);

        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Deserialize_Object_ShouldReturnDictionary()
    {
        var options = CreateOptions();

        const string json = """{ "a": 1, "b": true }""";

        var result = JsonSerializer.Deserialize<object>(json, options);

        var dict = Assert.IsType<Dictionary<string, object>>(result);

        Assert.Equal(1, dict["a"]);
        Assert.Equal(true, dict["b"]);
    }

    [Fact]
    public void Deserialize_NestedObject_ShouldWork()
    {
        var options = CreateOptions();

        const string json = """{ "a": { "b": 2 } }""";

        var result = JsonSerializer.Deserialize<object>(json, options);

        var dict = Assert.IsType<Dictionary<string, object>>(result);

        var nested = Assert.IsType<Dictionary<string, object>>(dict["a"]);

        Assert.Equal(2, nested["b"]);
    }

    [Fact]
    public void Deserialize_ListOfObject_ShouldWork()
    {
        var options = CreateOptions();

        const string json = "[1, true, \"abc\"]";

        var result = JsonSerializer.Deserialize<List<object>>(json, options);

        Assert.Equal(3, result!.Count);
        Assert.IsType<int>(result[0]);
        Assert.IsType<bool>(result[1]);
        Assert.IsType<string>(result[2]);
    }

    [Fact]
    public void Deserialize_DictionaryOfObject_ShouldWork()
    {
        var options = CreateOptions();

        const string json = """{ "x": 1, "y": "abc" }""";

        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);

        Assert.Equal(1, result!["x"]);
        Assert.Equal("abc", result["y"]);
    }

    [Fact]
    public void RoundTrip_ObjectGraph_ShouldWork()
    {
        var options = CreateOptions();

        var obj = new Dictionary<string, object>
        {
            ["a"] = 1,
            ["b"] = new List<object> { 1, 2, 3 },
            ["c"] = new Dictionary<string, object>
            {
                ["x"] = true
            }
        };

        var json = JsonSerializer.Serialize(obj, options);

        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);

        Assert.Equal(1, result!["a"]);
    }

    [Fact]
    public void Deserialize_ObjectOfArray_ShouldWork()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object[]>("[1, \"x\", false]", options);

        Assert.Equal(3, result!.Length);

        Assert.IsType<int>(result[0]);
        Assert.IsType<string>(result[1]);
        Assert.IsType<bool>(result[2]);
    }

    [Fact]
    public void Deserialize_IEnumerableOfObject_ShouldWork()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<IEnumerable<object>>("[1,2,3]", options);

        Assert.NotNull(result);

        var list = result.ToList();

        Assert.Equal(3, list.Count);
        Assert.All(list, x => Assert.IsType<int>(x));
    }

    [Fact]
    public void Deserialize_Nested_Containers()
    {
        var options = CreateOptions();

        const string json = """
{
    "a": [1,2],
    "b": { "c": true }
}
""";

        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options)!;

        var list = Assert.IsType<List<object>>(result["a"]);
        Assert.Equal(2, list.Count);

        var dict = Assert.IsType<Dictionary<string, object>>(result["b"]);
        Assert.True((bool)dict["c"]);
    }

    [Fact]
    public void Roundtrip_IEnumerable_Object()
    {
        var options = CreateOptions();

        IEnumerable<object> source = [1, "x", true];

        var json = JsonSerializer.Serialize(source, options);

        var result = JsonSerializer.Deserialize<IEnumerable<object>>(json, options)!;

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public void Deserialize_ScientificNotation_ShouldWork()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("1e3", options);

        Assert.IsType<double>(result);
        Assert.Equal(1000d, result);
    }

    [Fact]
    public void Deserialize_LargeInteger_ShouldFallbackToLongOrDouble()
    {
        var options = CreateOptions();

        const string json = "9223372036854775807"; // long.MaxValue

        var result = JsonSerializer.Deserialize<object>(json, options);

        Assert.IsType<long>(result);
    }

    [Fact]
    public void Deserialize_TooLargeInteger_ShouldBecomeDouble()
    {
        var options = CreateOptions();

        const string json = "922337203685477580799";

        var result = JsonSerializer.Deserialize<object>(json, options);

        Assert.IsType<double>(result);
    }

    [Fact]
    public void Deserialize_EmptyObject_ShouldWork()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("{}", options);

        var dict = Assert.IsType<Dictionary<string, object>>(result);
        Assert.Empty(dict);
    }

    [Fact]
    public void Deserialize_EmptyArray_ShouldWork()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<object>("[]", options);

        var list = Assert.IsType<List<object>>(result);
        Assert.Empty(list);
    }

    [Fact]
    public void Deserialize_DeepJson_ShouldRespectMaxDepth()
    {
        var options = CreateOptions();

        var json = new StringBuilder();
        for (var i = 0; i < 70; i++) json.Append("{\"a\":");
        json.Append('1');
        for (var i = 0; i < 70; i++) json.Append('}');

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<object>(json.ToString(), options));
    }

    [Fact]
    public void Deserialize_ArrayOfObjects_ShouldWork()
    {
        var options = CreateOptions();

        const string json = """[{"a":1},{"b":2}]""";

        var result = JsonSerializer.Deserialize<object>(json, options);

        var list = Assert.IsType<List<object>>(result);

        var d1 = Assert.IsType<Dictionary<string, object>>(list[0]);
        var d2 = Assert.IsType<Dictionary<string, object>>(list[1]);

        Assert.Equal(1, d1["a"]);
        Assert.Equal(2, d2["b"]);
    }

    [Fact]
    public void Deserialize_DateTimeOffsetString_ShouldStayStringOrDateTime()
    {
        var options = CreateOptions();

        const string json = "\"2024-01-01T12:00:00+02:00\"";

        var result = JsonSerializer.Deserialize<object>(json, options);

        Assert.True(result is DateTime or string);
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldThrow()
    {
        var options = CreateOptions();

        const string json = "{ \"a\": 1, }"; // trailing comma（默认不允许）

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<object>(json, options));
    }

    [Fact]
    public void Deserialize_MixedArray_ShouldPreserveTypes()
    {
        var options = CreateOptions();

        const string json = "[1, 1.5, true, \"x\"]";

        var result = JsonSerializer.Deserialize<object>(json, options);

        var list = Assert.IsType<List<object>>(result);

        Assert.IsType<int>(list[0]);
        Assert.IsType<double>(list[1]);
        Assert.IsType<bool>(list[2]);
        Assert.IsType<string>(list[3]);
    }
}
