namespace System.Text.Json;

public class IgnoreJsonConverterTests
{
    [Fact]
    public void ReadJson_ValueIsNull_Test()
    {
        const string json = "{\"ret_code\":20000000,\"msg\":\"success\",\"data\":null}";
        var obj = json.FromJson<TestModel>()!;
        Assert.Equal(20000000, obj.RetCode);
        Assert.Equal("success", obj.Msg);
    }

    [Fact]
    public void ReadJson_ValueIsNotNull_Test()
    {
        const string json = "{\"ret_code\":20000000,\"msg\":\"success\",\"data\":{}}";
        var obj = json.FromJson<TestModel>()!;
        Assert.Equal(20000000, obj.RetCode);
        Assert.Equal("success", obj.Msg);
    }

    [Fact]
    public void WriteJson_ValueIsNull_Test()
    {
        var obj = new TestModel { RetCode = 20000000, Msg = "success" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\",\"data\":null}", json);
    }

    [Fact]
    public void WriteJson_IgnoreNull_ValueIsNull_Test()
    {
        var obj = new TestModelIgnoreNull { RetCode = 20000000, Msg = "success" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\"}", json);
    }

    [Fact]
    public void WriteJson_ValueIsNotNull_Test()
    {
        var obj = new TestModel { RetCode = 20000000, Msg = "success", Data = "test" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\",\"data\":null}", json);
    }

    [Fact]
    public void WriteJson_IgnoreNull_ValueIsNotNull_Test()
    {
        var obj = new TestModelIgnoreNull { RetCode = 20000000, Msg = "success", Data = "test" };
        var json = obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.Equal("{\"ret_code\":20000000,\"msg\":\"success\",\"data\":null}", json);
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