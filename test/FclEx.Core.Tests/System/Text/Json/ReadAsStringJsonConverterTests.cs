using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Text.Json;

public class ReadAsStringJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new ReadAsStringJsonConverter() }
    };

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("123", "123")]
    [InlineData("123.45", "123.45")]
    [InlineData("true", "true")]
    [InlineData("false", "false")]
    [InlineData("null", null)]
    public void Read_PrimitiveValues_ReturnsExpectedString(string json, string? expected)
    {
        var result = JsonSerializer.Deserialize<string>(json, _options);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("9223372036854775808")]
    [InlineData("12345678901234567890.12345678901234567890")]
    [InlineData("1.2300e+100")]
    public void Read_Number_PreservesRawJsonText(string json)
    {
        var result = JsonSerializer.Deserialize<string>(json, _options);

        Assert.Equal(json, result);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("123", "123")]
    [InlineData("123.45", "123.45")]
    [InlineData("true", "true")]
    [InlineData("false", "false")]
    [InlineData("null", null)]
    public void Read_PrimitiveValues_ForProperty_ReturnsExpectedString(string json, string? expected)
    {
        var wrapped = $"{{\"Value\":{json}}}";
        var result = JsonSerializer.Deserialize<Wrapper>(wrapped, _options);
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Read_ObjectValue_ReturnsRawJsonText()
    {
        var json = "{\"a\":1,\"b\":\"text\"}";
        var wrapped = $"{{\"Value\":{json}}}";

        var result = JsonSerializer.Deserialize<Wrapper>(wrapped, _options);
        Assert.NotNull(result);
        Assert.Equal(json, result.Value);
    }

    [Fact]
    public void Read_ArrayValue_ReturnsRawJsonText()
    {
        var jsonArray = "[1,2,3]";
        var wrapped = $"{{\"Value\":{jsonArray}}}";
        var result = JsonSerializer.Deserialize<Wrapper>(wrapped, _options);
        Assert.NotNull(result);
        Assert.Equal(jsonArray, result.Value);
    }

    [Fact]
    public void Write_StringValue_SerializesAsJsonString()
    {
        var obj = new Wrapper { Value = "example" };
        var json = JsonSerializer.Serialize(obj, _options);
        Assert.Equal("{\"Value\":\"example\"}", json);
    }

    [Fact]
    public void Write_NullValue_SerializesAsNull()
    {
        var obj = new Wrapper { Value = null };
        var json = JsonSerializer.Serialize(obj, _options);
        Assert.Equal("{\"Value\":null}", json);
    }

    private class Wrapper
    {
        public string? Value { get; set; }
    }
}
