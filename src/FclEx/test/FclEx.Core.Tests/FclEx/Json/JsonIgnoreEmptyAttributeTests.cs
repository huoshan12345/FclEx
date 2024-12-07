namespace FclEx.Json;

public class JsonIgnoreEmptyAttributeTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddModifierForEmptyValue();

    private class SampleModel
    {
        [JsonIgnoreEmpty]
        public string EmptyString { get; set; } = string.Empty;
        [JsonIgnoreEmpty]
        public List<int> EmptyCollection { get; set; } = [];
        public string NonEmptyString { get; set; } = "Hello";
        public List<int> NonEmptyCollection { get; set; } = [1, 2, 3];
        public int Number { get; set; } = 42;
    }

    private static void Serialize_WithEmptyValues_ShouldIgnoreEmptyStringAndCollection(JsonSerializerOptions option)
    {
        var model = new SampleModel();
        var json = JsonSerializer.Serialize(model, option);
        var element = json.ToJsonElement();

        Assert.False(element.TryGetProperty(nameof(SampleModel.EmptyString), out _));
        Assert.False(element.TryGetProperty(nameof(SampleModel.EmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.Number), out _));
    }

    [Fact]
    public void Serialize_WithEmptyValues_ShouldIgnoreEmptyStringAndCollection_CustomOptions()
    {
        Serialize_WithEmptyValues_ShouldIgnoreEmptyStringAndCollection(_options);
    }

    [Fact]
    public void Serialize_WithEmptyValues_ShouldIgnoreEmptyStringAndCollection_OptionsFromJsonHelper()
    {
        Serialize_WithEmptyValues_ShouldIgnoreEmptyStringAndCollection(JsonHelper.GetOptions());
    }

    [Fact]
    public void Serialize_WithNonEmptyValues_ShouldIncludeNonEmptyProperties()
    {
        var model = new SampleModel
        {
            EmptyString = "Not empty",
            EmptyCollection = [10, 20],
        };
        var json = JsonSerializer.Serialize(model, _options);
        var element = json.ToJsonElement();

        Assert.True(element.TryGetProperty(nameof(SampleModel.EmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.Number), out _));
    }

    [Fact]
    public void Serialize_WithCustomOptions_ShouldNotApplyAttributeByDefault()
    {
        var customOptions = new JsonSerializerOptions(); // Without calling AddModifierForEmptyValue
        var model = new SampleModel();
        var json = JsonSerializer.Serialize(model, customOptions);
        var element = json.ToJsonElement();

        Assert.True(element.TryGetProperty(nameof(SampleModel.EmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.Number), out _));
    }
}