namespace System.Text.Json;

public class JsonIgnoreEmptyAttributeTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().AddModifierForEmptyValue();

    private class SampleModel
    {
        [JsonIgnoreEmpty]
        public string IgnoredEmptyString { get; set; } = string.Empty;
        [JsonIgnoreEmpty]
        public List<int> IgnoredEmptyCollection { get; set; } = [];
        public string NonEmptyString { get; set; } = "Hello";
        public List<int> NonEmptyCollection { get; set; } = [1, 2, 3];
        public int Number { get; set; } = 42;

        public string EmittedEmptyString { get; set; } = string.Empty;
        public List<int> EmittedEmptyCollection { get; set; } = [];
    }

    private static void Serialize_WithEmptyValues_ShouldIgnoreEmptyStringAndCollection(JsonSerializerOptions option)
    {
        var model = new SampleModel();
        var json = JsonSerializer.Serialize(model, option);
        var element = json.ToJsonElement();

        Assert.False(element.TryGetProperty(nameof(SampleModel.IgnoredEmptyString), out _));
        Assert.False(element.TryGetProperty(nameof(SampleModel.IgnoredEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.Number), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmittedEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmittedEmptyCollection), out _));
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
            IgnoredEmptyString = "Not empty",
            IgnoredEmptyCollection = [10, 20],
        };
        var json = JsonSerializer.Serialize(model, _options);
        var element = json.ToJsonElement();

        Assert.True(element.TryGetProperty(nameof(SampleModel.IgnoredEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.IgnoredEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.Number), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmittedEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmittedEmptyCollection), out _));
    }

    [Fact]
    public void Serialize_WithCustomOptions_ShouldNotApplyAttributeByDefault()
    {
        var model = new SampleModel();
        var json = JsonSerializer.Serialize(model, new JsonSerializerOptions());
        var element = json.ToJsonElement();

        Assert.True(element.TryGetProperty(nameof(SampleModel.IgnoredEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.IgnoredEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.NonEmptyCollection), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.Number), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmittedEmptyString), out _));
        Assert.True(element.TryGetProperty(nameof(SampleModel.EmittedEmptyCollection), out _));
    }
}