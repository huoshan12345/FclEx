using FclEx.Extensions;
using Newtonsoft.Json;

namespace FclEx.Json;

public class IgnoreJsonConverterTests
{
    [Fact]
    public void ReadJson_Test()
    {
        var json = "{\"retcode\":20000000,\"msg\":\"succ\",\"data\":null}";
        var obj = json.ToJToken().ToObject<Tester>()!;
        Assert.Equal(20000000, obj.Retcode);
        Assert.Equal("succ", obj.Msg);
    }

    [Fact]
    public void WriteJson_Test()
    {
        var obj = new Tester { Retcode = 20000000, Msg = "succ" };
        var json = obj.ToJsonCamel();
        Assert.Equal("{\"retcode\":20000000,\"msg\":\"succ\",\"data\":null}", json);
    }

    public class Tester
    {
        [JsonProperty("retcode")]
        public long Retcode { get; set; }
        [JsonProperty("msg")]
        public string? Msg { get; set; }
        [JsonProperty("data")]
        public Unit Data { get; set; }
    }
}