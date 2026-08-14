namespace System.Text.Json;

public class BooleanJsonConverterTests
{
    [Fact]
    public void StrictConverterRejectsNull()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new BooleanJsonConverter());

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<bool>("null", options));
    }

    [Fact]
    public void PermissiveConverterTreatsNullAsFalse()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new BooleanJsonConverter(treatNullAsFalse: true));

        Assert.False(JsonSerializer.Deserialize<bool>("null", options));
    }

    [Fact]
    public void AllowBoolFromStringUsesNullAsFalseConverter()
    {
        var options = JsonHelper.GetOptions(new() { AllowBoolFromString = true });

        var model = JsonSerializer.Deserialize<BooleanModel>("""{"Value":null}""", options);

        Assert.NotNull(model);
        Assert.False(model.Value);
    }

    [Fact]
    public void ParameterlessConstructorSupportsConverterAttribute()
    {
        var model = JsonSerializer.Deserialize<AttributeBooleanModel>("""{"Value":"true"}""");

        Assert.True(model!.Value);
    }

    private sealed class BooleanModel
    {
        public bool Value { get; set; } = true;
    }

    private sealed class AttributeBooleanModel
    {
        [JsonConverter(typeof(BooleanJsonConverter))]
        public bool Value { get; set; }
    }
}
