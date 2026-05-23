namespace FclEx.NewtonsoftJson;

public class IgnoreJsonConverterTests
{
    [Fact]
    public void Read_Test()
    {
        var json = "{\"retcode\":20000000,\"msg\":\"succ\",\"data\":null}";
        var obj = json.ToJToken().ToObject<TestModel>()!;
        Assert.Equal(20000000, obj.RetCode);
        Assert.Equal("succ", obj.Msg);
    }

    [Fact]
    public void Write_Test()
    {
        var obj = new TestModel { RetCode = 20000000, Msg = "succ", Data = "xxxxxxxx" };
        var json = obj.ToNewtonsoftJson(new NewtonsoftJsonOptions { CamelCase = true });
        Assert.Equal("{\"retcode\":20000000,\"msg\":\"succ\",\"data\":null}", json);
    }

    public class TestModel
    {
        [JsonProperty("retcode")]
        public long RetCode { get; set; }
        [JsonProperty("msg")]
        public string? Msg { get; set; }
        [JsonProperty("data"), JsonConverter(typeof(IgnoreJsonConverter))]
        public string? Data { get; set; }
    }
}