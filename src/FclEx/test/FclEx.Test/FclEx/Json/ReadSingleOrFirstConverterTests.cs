using System.Linq;
using FclEx.Json.Converters;
using Newtonsoft.Json;

namespace FclEx.Json;

public class ReadSingleOrFirstConverterTests
{
    private class TesterOfSingle
    {
        public string? Id { get; set; }
    }

    private class TesterOfArray
    {
        public string?[]? Id { get; set; }
    }

    private class Tester
    {
        [JsonConverter(typeof(ReadSingleOrFirstConverter))]
        public string? Id { get; set; }
    }

    [Fact]
    public void ReadSingle_Test()
    {
        var obj = new TesterOfSingle() { Id = "1" };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Equal(obj.Id, actual.Id);
    }

    [Fact]
    public void ReadSingle_Null_Test()
    {
        var obj = new TesterOfSingle() { Id = null };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Null(actual.Id);
    }

    [Fact]
    public void ReadFirst_Test()
    {
        var obj = new TesterOfArray { Id = Enumerable.Range(1, 10).Select(m => m.ToString()).ToArray() };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Equal(obj.Id.First(), actual.Id);
    }

    [Fact]
    public void ReadFirst_Null_Test()
    {
        var obj = new TesterOfArray { Id = null };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Null(actual.Id);
    }

    [Fact]
    public void ReadFirst_Value_Null_Test()
    {
        var obj = new TesterOfArray { Id = new[] { null, "2", "3" } };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Null(actual.Id);
    }
}