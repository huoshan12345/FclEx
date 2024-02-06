namespace FclEx.Json;

public class ReadSingleOrLastConverterTests
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
        [JsonConverter(typeof(ReadSingleOrLastConverter))]
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
    public void ReadLast_Test()
    {
        var obj = new TesterOfArray { Id = Enumerable.Range(1, 10).Select(m => m.ToString()).ToArray() };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Equal(obj.Id.Last(), actual.Id);
    }

    [Fact]
    public void ReadLast_Null_Test()
    {
        var obj = new TesterOfArray { Id = null };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Null(actual.Id);
    }

    [Fact]
    public void ReadLast_Value_Null_Test()
    {
        var obj = new TesterOfArray { Id = new[] { "1", "2", null } };
        var json = obj.ToJson();
        var actual = json.ToJToken().ToObject<Tester>()!;
        Assert.Null(actual.Id);
    }
}