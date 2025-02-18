namespace System.Text.Json;

public class JsonHelperTests
{
    public class TestModel
    {
        public string String { get; set; } = "";
        public int Int { get; set; }
        public bool Bool { get; set; }
    }

    [Fact]
    public void ReadingNull_WithIgnoreReadingNullOption()
    {
        const string json = """{ "String": null }""";
        var options = JsonHelper.GetOptions();
        var model = JsonSerializer.Deserialize<TestModel>(json, options);
        Assert.NotNull(model);
        Assert.Equal("", model.String);
    }

    [Fact]
    public void ReadingNull_WithIgnoreReadingNullFalseOption()
    {
        const string json = """{ "String": null }""";
        var options = JsonHelper.GetOptions(new() { IgnoreReadingNull = false });
        var model = JsonSerializer.Deserialize<TestModel>(json, options);
        Assert.NotNull(model);
        Assert.Null(model.String);
    }

    [Fact]
    public void AllowBoolFromString_WithOptionTrue_Success()
    {
        const string json = """{ "Bool": "true" }""";
        var options = JsonHelper.GetOptions();
        var model = JsonSerializer.Deserialize<TestModel>(json, options);
        Assert.NotNull(model);
        Assert.True(model.Bool);
    }

    [Fact]
    public void AllowBoolFromString_WithOptionFalse_ThrowsException()
    {
        const string json = """{ "Bool": "true" }""";
        var options = JsonHelper.GetOptions(new() { AllowBoolFromString = false });
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestModel>(json, options));
        Assert.Contains("The JSON value could not be converted to System.Boolean", ex.Message);
    }

    [Fact]
    public void AllowNumberFromString_WithOptionTrue_Success()
    {
        const string json = """{ "Int": "1" }""";
        var options = JsonHelper.GetOptions();
        var model = JsonSerializer.Deserialize<TestModel>(json, options);
        Assert.NotNull(model);
        Assert.Equal(1, model.Int);
    }

    [Fact]
    public void AllowNumberFromString_WithOptionFalse_ThrowsException()
    {
        const string json = """{ "Int": "1" }""";
        var options = JsonHelper.GetOptions(new() { AllowNumberFromString = false });
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestModel>(json, options));
        Assert.Contains("The JSON value could not be converted to System.Int32", ex.Message);
    }
}