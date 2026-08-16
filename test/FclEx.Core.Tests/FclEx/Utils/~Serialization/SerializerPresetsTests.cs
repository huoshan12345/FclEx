namespace FclEx.Utils;

public class SerializerPresetsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("plain text")]
    [InlineData("123")]
    [InlineData("{\"looks\":\"like json\"}")]
    [InlineData("  \t\n  ")]
    public void StringOrJson_ShouldPreserveStringsVerbatim(string value)
    {
        var serializer = SerializerPresets.StringOrJson;

        var data = serializer.Serialize(value);

        Assert.Equal(value, data);
        Assert.Equal(value, serializer.Deserialize<string>(data));
    }

    [Fact]
    public void StringOrJson_ShouldDelegateOtherTypesToJson()
    {
        var serializer = SerializerPresets.StringOrJson;
        var value = new Sample(42, "value");

        var data = serializer.Serialize(value);

        Assert.Equal(value.ToJson(), data);
        Assert.Equal(value, serializer.Deserialize<Sample>(data));
    }

    [Fact]
    public void Utf8StringOrJson_ShouldEncodeTheComposedStringSerializerOutput()
    {
        var serializer = SerializerPresets.Utf8StringOrJson;
        const string value = "原始文本 { not json }";

        var data = serializer.Serialize(value);

        Assert.Equal(Encoding.UTF8.GetBytes(value), data.ToArray());
        Assert.Equal(value, serializer.Deserialize<string>(data));
    }

    [Fact]
    public void Utf8StringOrJson_ShouldRoundTripJsonValues()
    {
        var serializer = SerializerPresets.Utf8StringOrJson;
        var value = new Sample(42, "value");

        var data = serializer.Serialize(value);

        Assert.Equal(Encoding.UTF8.GetBytes(value.ToJson()), data.ToArray());
        Assert.Equal(value, serializer.Deserialize<Sample>(data));
    }

    private sealed record Sample(int Number, string Text);
}
