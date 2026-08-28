namespace System.Text.Json;

public class OptionalJsonConverterTests
{
    [Fact]
    public void Serialize_Value_ShouldWriteContainedValue()
    {
        var optional = Optional.Some(42);

        var json = JsonSerializer.Serialize(optional);

        Assert.Equal("42", json);
    }

    [Fact]
    public void Serialize_None_ShouldWriteNull()
    {
        var optional = Optional.None<int>();

        var json = JsonSerializer.Serialize(optional);

        Assert.Equal("null", json);
    }

    [Fact]
    public void Deserialize_Value_ShouldCreateOptionalWithValue()
    {
        var optional = JsonSerializer.Deserialize<Optional<int>>("42");

        Assert.True(optional.HasValue);
        Assert.Equal(42, optional.Value);
    }

    [Fact]
    public void Deserialize_Null_ShouldCreateOptionalWithoutValue()
    {
        var optional = JsonSerializer.Deserialize<Optional<int>>("null");

        Assert.False(optional.HasValue);
    }

    [Fact]
    public void RoundTrip_ShouldUseConfiguredConverterForContainedValue()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UpperCaseStringJsonConverter());
        var optional = Optional.Some("value");

        var json = JsonSerializer.Serialize(optional, options);
        var result = JsonSerializer.Deserialize<Optional<string>>(json, options);

        Assert.Equal("\"VALUE\"", json);
        Assert.True(result.HasValue);
        Assert.Equal("value", result.Value);
    }

    [Fact]
    public void Deserialize_InvalidContainedValue_ShouldThrow()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Optional<int>>("\"invalid\""));
    }

    [Fact]
    public void CanConvert_ShouldOnlyAcceptOptionalTypes()
    {
        var converter = OptionalJsonConverter.Instance;

        Assert.True(converter.CanConvert(typeof(Optional<int>)));
        Assert.False(converter.CanConvert(typeof(int)));
        Assert.False(converter.CanConvert(typeof(List<>)));
    }

    private sealed class UpperCaseStringJsonConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString()?.ToLowerInvariant();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUpperInvariant());
        }
    }
}
