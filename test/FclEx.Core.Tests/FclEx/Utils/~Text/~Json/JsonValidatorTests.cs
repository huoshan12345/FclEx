namespace FclEx.Utils;

public class JsonValidatorTests
{
    public static TheoryData<string> ValidJson =>
    [
        "null",
        "true",
        "false",
        "0",
        "-0",
        "1234567890",
        "-1234567890",
        "0.125",
        "-12.34",
        "1e10",
        "1E+10",
        "1e-10",
        "\"text\"",
        "\"\\\"\\\\\\/\\b\\f\\n\\r\\t\"",
        "\"\\u0041\"",
        "[]",
        "{}",
        "[null,true,false,0,\"text\",[],{}]",
        "{\"name\":\"value\",\"items\":[1,2,3],\"nested\":{\"ok\":true}}",
        "{\"duplicate\":1,\"duplicate\":2}",
        " \t\r\n [ 1, { \"value\" : null } ] \t\r\n "
    ];

    public static TheoryData<string> InvalidJson =>
    [
        "",
        " \t\r\n ",
        "Null",
        "TRUE",
        "'text'",
        "{name:1}",
        "{\"name\"}",
        "{\"name\":}",
        "{\"name\":1,}",
        "[1,]",
        "[1 2]",
        "[",
        "{",
        "\"unterminated",
        "\"\\x\"",
        "\"\\u12G4\"",
        "\"line\nbreak\"",
        "+1",
        "01",
        "-01",
        ".1",
        "1.",
        "1e",
        "1e+",
        "NaN",
        "Infinity",
        "true false",
        "nullx",
        "/*comment*/null",
        "//comment\nnull"
    ];

    public static TheoryData<string> JsonWithNonJsonWhitespace =>
    [
        "\u000Bnull",
        "null\u000C",
        "\u0085null",
        "null\u00A0",
        "[\u20031]"
    ];

    public static TheoryData<string> JsonWithNonAsciiDigits =>
    [
        "١",
        "-١",
        "١.٢",
        "1e١",
        "[١]"
    ];

    [Theory]
    [MemberData(nameof(ValidJson))]
    public void IsValid_ValidJson_ReturnsTrue(string json)
    {
        Assert.True(JsonValidator.IsValid(json));
    }

    [Theory]
    [MemberData(nameof(InvalidJson))]
    public void IsValid_InvalidJson_ReturnsFalse(string json)
    {
        Assert.False(JsonValidator.IsValid(json));
    }

    [Theory]
    [MemberData(nameof(JsonWithNonJsonWhitespace))]
    public void IsValid_NonJsonWhitespace_ReturnsFalse(string json)
    {
        Assert.False(JsonValidator.IsValid(json));
    }

    [Theory]
    [MemberData(nameof(JsonWithNonAsciiDigits))]
    public void IsValid_NonAsciiDigits_ReturnsFalse(string json)
    {
        Assert.False(JsonValidator.IsValid(json));
    }

    [Fact]
    public void IsValid_ArrayAtMaxDepth_ReturnsTrue()
    {
        var json = NestValue("0", '[', ']', JsonValidator.MaxDepth);

        Assert.True(JsonValidator.IsValid(json));
    }

    [Fact]
    public void IsValid_ArrayBeyondMaxDepth_ReturnsFalse()
    {
        var json = NestValue("0", '[', ']', JsonValidator.MaxDepth + 1);

        Assert.False(JsonValidator.IsValid(json));
    }

    [Fact]
    public void IsValid_ObjectAtMaxDepth_ReturnsTrue()
    {
        var json = NestObject(JsonValidator.MaxDepth);

        Assert.True(JsonValidator.IsValid(json));
    }

    [Fact]
    public void IsValid_ObjectBeyondMaxDepth_ReturnsFalse()
    {
        var json = NestObject(JsonValidator.MaxDepth + 1);

        Assert.False(JsonValidator.IsValid(json));
    }

    private static string NestValue(string value, char opening, char closing, int depth)
    {
        return new string(opening, depth) + value + new string(closing, depth);
    }

    private static string NestObject(int depth)
    {
        var value = "0";
        for (var i = 0; i < depth; i++)
            value = "{\"value\":" + value + "}";

        return value;
    }
}
