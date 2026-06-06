namespace FclEx.Utils;

public class UnitTests
{
    [Fact]
    public void Equal_Test()
    {
        Assert.Equal(new Unit(), new Unit());
    }

    [Fact]
    public void ToJson_Test()
    {
        var json = new Unit().ToJson();
        Assert.Equal("", json);
    }

    [Fact]
    public void ToJson_AsProperty_Test()
    {
        var json = new TestModel().ToJson();
        Assert.Equal("{\"Unit\":null,\"Units\":[null,null]}", json);
    }

    [Fact]
    public void FromJson_AsProperty_Test()
    {
        const string json = "{\"Unit\":{},\"Units\":[{},null]}";
        var model = json.FromJson<TestModel>()!;
        Assert.Equal(default, model.Unit);
        Assert.Equal(new Unit[] { default, default }, model.Units);
    }
}

file class TestModel
{
    public Unit Unit { get; set; }
    public Unit[] Units { get; set; } = [default, default];
}