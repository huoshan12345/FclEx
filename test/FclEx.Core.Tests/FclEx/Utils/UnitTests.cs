namespace FclEx.Utils;

public class UnitTests
{
    [Fact]
    public void Default_ShouldEqualNewUnit()
    {
        Assert.Equal(new Unit(), Unit.Default);
        Assert.Equal(default, Unit.Default);
    }

    [Fact]
    public void GetHashCode_ShouldReturnZero()
    {
        Assert.Equal(0, Unit.Default.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnUnitLiteral()
    {
        Assert.Equal("()", Unit.Default.ToString());
    }

    [Fact]
    public void Serialize_AsRootValue_ShouldWriteEmptyPayload()
    {
        var json = new Unit().ToJson();

        Assert.Equal("", json);
    }

    [Fact]
    public void Serialize_AsProperty_ShouldWriteNull()
    {
        var json = new TestModel().ToJson();

        Assert.Equal("{\"Unit\":null,\"Units\":[null,null]}", json);
    }

    [Fact]
    public void Deserialize_AsProperty_ShouldIgnoreJsonValues()
    {
        const string json = "{\"Unit\":{\"ignored\":true},\"Units\":[{},null,{\"items\":[1,2]}]}";
        var model = json.FromJson<TestModel>()!;

        Assert.Equal(default, model.Unit);
        Assert.Equal([default, default, default], model.Units);
    }
}

file class TestModel
{
    public Unit Unit { get; set; }
    public Unit[] Units { get; set; } = [default, default];
}
