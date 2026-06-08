namespace System.Text.Json;

public class IgnoreJsonConverterTests
{
    public static TheoryData<string> IgnoredJsonValues =>
    [
        "null",
        "{}",
        "{\"items\":[1,true,null,{\"name\":\"test\"}]}",
        "[]",
        "[{\"id\":1}]",
        "\"test\"",
        "123",
        "true"
    ];

    [Theory]
    [MemberData(nameof(IgnoredJsonValues))]
    public void Deserialize_DataValue_ShouldBeIgnored(string dataJson)
    {
        var json = "{\"ret_code\":20000000,\"msg\":\"success\",\"data\":" + dataJson + "}";
        var obj = json.FromJson<TestModel>();

        Assert.NotNull(obj);
        Assert.Equal(20000000, obj.RetCode);
        Assert.Equal("success", obj.Msg);
        Assert.Null(obj.Data);
    }

    [Fact]
    public void Serialize_DataValueIsNull_ShouldWriteNull()
    {
        var obj = new TestModel { RetCode = 20000000, Msg = "success" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\",\"data\":null}", json);
    }

    [Fact]
    public void Serialize_DataValueIsNotNull_ShouldWriteNull()
    {
        var obj = new TestModel { RetCode = 20000000, Msg = "success", Data = "test" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\",\"data\":null}", json);
    }

    [Fact]
    public void Serialize_WhenWritingNullAndDataValueIsNull_ShouldOmitProperty()
    {
        var obj = new TestModelIgnoreNull { RetCode = 20000000, Msg = "success" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\"}", json);
    }

    [Fact]
    public void Serialize_WhenWritingNullAndDataValueIsNotNull_ShouldWriteConverterNull()
    {
        var obj = new TestModelIgnoreNull { RetCode = 20000000, Msg = "success", Data = "test" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\",\"data\":null}", json);
    }

    [Fact]
    public void Serialize_JsonIgnoredProperty_ShouldOmitProperty()
    {
        var obj = new TestModelJsonIgnored { RetCode = 20000000, Msg = "success", Data = "test" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\"}", json);
    }
}

file class TestModel
{
    [JsonPropertyName("ret_code")]
    public long RetCode { get; set; }
    [JsonPropertyName("msg")]
    public string? Msg { get; set; }
    [JsonConverter(typeof(IgnoreJsonConverter))]
    public object? Data { get; set; }
}

file class TestModelIgnoreNull
{
    [JsonPropertyName("ret_code")]
    public long RetCode { get; set; }
    [JsonPropertyName("msg")]
    public string? Msg { get; set; }
    [JsonConverter(typeof(IgnoreJsonConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }
}

file class TestModelJsonIgnored
{
    [JsonPropertyName("ret_code")]
    public long RetCode { get; set; }
    [JsonPropertyName("msg")]
    public string? Msg { get; set; }
    [JsonConverter(typeof(IgnoreJsonConverter))]
    [JsonIgnore]
    public object? Data { get; set; }
}
