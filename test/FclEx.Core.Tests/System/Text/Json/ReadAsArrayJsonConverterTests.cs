namespace System.Text.Json;

public class ReadAsArrayJsonConverterTests
{
    private const string ModelWithArrays = """
                                                 {
                                                   "Array": ["1", "2"],
                                                   "List": ["1", "2"]
                                                 }
                                                 """;

    private const string ModelWithSingleValues = """
                                                       {
                                                         "Array": "1",
                                                         "List": "1"
                                                       }
                                                       """;

    [Theory]
    [InlineData(typeof(ReadAsArrayJsonConverter))]
    [InlineData(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
    public void ReadRootCollectionFromSingleValue(Type converterType)
    {
        var value = JsonSerializer.Deserialize<TestModel[]>(ModelWithArrays, CreateOptions(converterType));

        var model = Assert.Single(value!);
        Assert.Equal(["1", "2"], model.Array!);
        Assert.Equal(["1", "2"], model.List);
    }

    [Theory]
    [InlineData(typeof(ReadAsArrayJsonConverter))]
    [InlineData(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
    public void ReadCollectionMembersFromArrays(Type converterType)
    {
        var model = JsonSerializer.Deserialize<TestModel>(ModelWithArrays, CreateOptions(converterType));

        Assert.Equal(["1", "2"], model!.Array!);
        Assert.Equal(["1", "2"], model.List);
    }

    [Theory]
    [InlineData(typeof(ReadAsArrayJsonConverter))]
    [InlineData(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
    public void ReadCollectionMembersFromSingleValues(Type converterType)
    {
        var model = JsonSerializer.Deserialize<TestModel>(ModelWithSingleValues, CreateOptions(converterType));

        Assert.Equal(["1"], model!.Array!);
        Assert.Equal(["1"], model.List);
    }

    [Theory]
    [InlineData(typeof(ReadAsArrayJsonConverter))]
    [InlineData(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
    public void WriteUsesNormalCollectionRepresentation(Type converterType)
    {
        var value = new TestModel
        {
            Array = ["1", "2"],
            List = ["3", "4"],
        };

        var json = JsonSerializer.Serialize(value, CreateOptions(converterType));

        Assert.Equal("""{"Array":["1","2"],"List":["3","4"]}""", json);
    }

    [Theory]
    [InlineData(typeof(ReadAsArrayJsonConverter))]
    [InlineData(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
    public void ReadAndWriteNestedCollections(Type converterType)
    {
        var options = CreateOptions(converterType);

        var value = JsonSerializer.Deserialize<List<int[]>>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        var inner = Assert.Single(value!);
        Assert.Equal([1], inner);
        Assert.Equal("[[1]]", json);
    }

    [Theory]
    [InlineData(typeof(ReadAsArrayJsonConverter))]
    [InlineData(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
    public void ReadAndWriteUsingDefaultQueueConverter(Type converterType)
    {
        var options = CreateOptions(converterType);

        var value = JsonSerializer.Deserialize<Queue<int>>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(1, Assert.Single(value!));
        Assert.Equal("[1]", json);
    }

    [Theory]
    [InlineData(typeof(PropertyModelUsingPublicApiConverter))]
    [InlineData(typeof(PropertyModelUsingBuiltInConverter))]
    public void PropertyAttributeSupportsSingleValues(Type modelType)
    {
        var model = (IPropertyModel)JsonSerializer.Deserialize(ModelWithSingleValues, modelType, JsonHelper.GetOptions())!;
        var json = JsonSerializer.Serialize(model, modelType, JsonHelper.GetOptions());

        Assert.Equal(["1"], model.Array!);
        Assert.Equal(["1"], model.List);
        Assert.Equal("""{"Array":["1"],"List":["1"]}""", json);
    }

    [Fact]
    public void PublicApiConverterComposesElementConverter()
    {
        var options = CreateOptions(
            typeof(ReadAsArrayJsonConverter),
            IncrementingIntJsonConverter.Instance);

        var value = JsonSerializer.Deserialize<int[]>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(2, Assert.Single(value!));
        Assert.Equal("[1]", json);
    }

    [Fact]
    public void BuiltInConverterBypassesElementConverter()
    {
        var options = CreateOptions(
            typeof(ReadAsArrayUsingBuiltInJsonConverter),
            IncrementingIntJsonConverter.Instance);

        var value = JsonSerializer.Deserialize<int[]>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(1, Assert.Single(value!));
        Assert.Equal("[1]", json);
    }

    [Fact]
    public void PublicApiConverterComposesNextCollectionConverter()
    {
        var options = CreateOptions(
            typeof(ReadAsArrayJsonConverter),
            QueueJsonConverter.Instance);

        var value = JsonSerializer.Deserialize<Queue<int>>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(2, Assert.Single(value!));
        Assert.Equal("\"2\"", json);
    }

    [Fact]
    public void BuiltInConverterBypassesNextCollectionConverter()
    {
        var options = CreateOptions(
            typeof(ReadAsArrayUsingBuiltInJsonConverter),
            QueueJsonConverter.Instance);

        var value = JsonSerializer.Deserialize<Queue<int>>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(1, Assert.Single(value!));
        Assert.Equal("[1]", json);
    }

    [Fact]
    public void ConvertersAdvertiseTheirDifferentScopes()
    {
        var publicApiConverter = ReadAsArrayJsonConverter.Instance;
        var builtInConverter = ReadAsArrayUsingBuiltInJsonConverter.Instance;

        Assert.False(publicApiConverter.CanConvert(typeof(string)));
        Assert.False(publicApiConverter.CanConvert(typeof(Dictionary<string, int>)));
        Assert.False(publicApiConverter.CanConvert(typeof(int[,])));
        Assert.True(publicApiConverter.CanConvert(typeof(Queue<int>)));

        Assert.True(builtInConverter.CanConvert(typeof(string)));
        Assert.True(builtInConverter.CanConvert(typeof(Dictionary<string, int>)));
        Assert.True(builtInConverter.CanConvert(typeof(int[,])));
        Assert.True(builtInConverter.CanConvert(typeof(Queue<int>)));
    }

    [Fact]
    public void PublicApiConverterRejectsTypeLevelRegistration()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => JsonSerializer.Deserialize<AttributedListUsingPublicApiConverter>("1"));

        Assert.Contains("Apply ReadAsArrayJsonConverter to a property", exception.Message);
    }

    [Fact]
    public void BuiltInConverterSupportsTypeLevelRegistration()
    {
        var value = JsonSerializer.Deserialize<AttributedListUsingBuiltInConverter>("1");
        var json = JsonSerializer.Serialize(value);

        Assert.Equal(1, Assert.Single(value!));
        Assert.Equal("[1]", json);
    }

    private static JsonSerializerOptions CreateOptions(Type converterType, params JsonConverter[] additionalConverters)
    {
        var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
        return JsonHelper.GetOptions().AddConverters([converter, .. additionalConverters]);
    }

    public sealed class TestModel
    {
        public string[]? Array { get; set; }
        public List<string>? List { get; set; }
    }

    public interface IPropertyModel
    {
        string[]? Array { get; }
        List<string>? List { get; }
    }

    public sealed class PropertyModelUsingPublicApiConverter : IPropertyModel
    {
        [JsonConverter(typeof(ReadAsArrayJsonConverter))]
        public string[]? Array { get; set; }

        [JsonConverter(typeof(ReadAsArrayJsonConverter))]
        public List<string>? List { get; set; }
    }

    public sealed class PropertyModelUsingBuiltInConverter : IPropertyModel
    {
        [JsonConverter(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
        public string[]? Array { get; set; }

        [JsonConverter(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
        public List<string>? List { get; set; }
    }

    private sealed class IncrementingIntJsonConverter : JsonConverter<int>
    {
        public static readonly IncrementingIntJsonConverter Instance = new();

        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetInt32() + 1;

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value - 1);
    }

    private sealed class QueueJsonConverter : JsonConverter<Queue<int>>
    {
        public static readonly QueueJsonConverter Instance = new();

        public override Queue<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Assert.Equal(JsonTokenType.StartArray, reader.TokenType);
            Assert.True(reader.Read());
            var value = reader.GetInt32() + 1;
            Assert.True(reader.Read());
            Assert.Equal(JsonTokenType.EndArray, reader.TokenType);
            return new Queue<int>([value]);
        }

        public override void Write(Utf8JsonWriter writer, Queue<int> value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Single().ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [JsonConverter(typeof(ReadAsArrayJsonConverter))]
    private sealed class AttributedListUsingPublicApiConverter : List<int>;

    [JsonConverter(typeof(ReadAsArrayUsingBuiltInJsonConverter))]
    private sealed class AttributedListUsingBuiltInConverter : List<int>;
}
