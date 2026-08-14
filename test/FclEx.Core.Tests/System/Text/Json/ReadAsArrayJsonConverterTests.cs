namespace System.Text.Json;

public class ReadAsArrayJsonConverterTests
{
    public class TestModel
    {
        [JsonConverter(typeof(ReadAsArrayJsonConverter))]
        public string?[]? Array { get; set; }
        [JsonConverter(typeof(ReadAsArrayJsonConverter))]
        public List<string>? List { get; set; }
    }

    [Fact]
    public void Read_Model_FromSingleValue_Test()
    {
        const string json = """
                            {
                            	"Array": ["1", "2"],
                            	"List": ["1", "2"]
                            }
                            """;

        var array = json.FromJson<TestModel[]>(JsonHelper.GetOptions().AddConverters([new ReadAsArrayJsonConverter()]))!;

        Assert.NotNull(array);
        Assert.Single(array);

        var obj = array[0];
        Assert.Equal(new[] { "1", "2" }, obj.Array);
        Assert.Equal(new[] { "1", "2" }, obj.List);
    }

    [Fact]
    public void Read_Member_FromArray_Test()
    {
        const string json = """
                            {
                            	"Array": ["1", "2"],
                            	"List": ["1", "2"]
                            }
                            """;
        var obj = json.FromJson<TestModel>()!;
        Assert.NotNull(obj);
        Assert.NotNull(obj.Array);
        Assert.Equal(new[] { "1", "2" }, obj.Array);
        Assert.Equal(new[] { "1", "2" }, obj.List);
    }

    [Fact]
    public void Read_Member_FromSingleValue_Test()
    {
        const string json = """
                            {
                            	"Array": "1",
                            	"List": "1"
                            }
                            """;
        var obj = json.FromJson<TestModel>()!;
        Assert.NotNull(obj);
        Assert.NotNull(obj.Array);
        Assert.Equal(new[] { "1" }, obj.Array);
        Assert.Equal(new[] { "1" }, obj.List);
    }


    [Fact]
    public void Write_Test()
    {
        var obj = new TestModel
        {
            Array = ["1", "2"],
            List = ["1", "2"],
        };
        const string json = """{"Array":["1","2"],"List":["1","2"]}""";
        Assert.Equal(json, obj.ToJson());
    }

    [Fact]
    public void Write_WithGlobalConverter_DoesNotReenterConverter()
    {
        var options = JsonHelper.GetOptions().AddConverters([ReadAsArrayJsonConverter.Instance]);
        var value = new[]
        {
            new TestModel
            {
                Array = ["1", "2"],
                List = ["3", "4"],
            }
        };

        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal("""[{"Array":["1","2"],"List":["3","4"]}]""", json);
    }

    [Fact]
    public void ReadAndWrite_NestedCollections()
    {
        var options = JsonHelper.GetOptions().AddConverters([ReadAsArrayJsonConverter.Instance]);

        var value = JsonSerializer.Deserialize<List<int[]>>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        var inner = Assert.Single(value!);
        Assert.Equal([1], inner);
        Assert.Equal("[[1]]", json);
    }

    [Fact]
    public void ReadAndWrite_ComposesElementConverter()
    {
        var options = JsonHelper.GetOptions().AddConverters([
            ReadAsArrayJsonConverter.Instance,
            IncrementingIntJsonConverter.Instance,
        ]);

        var value = JsonSerializer.Deserialize<int[]>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(2, Assert.Single(value!));
        Assert.Equal("[1]", json);
    }

    [Fact]
    public void CanConvert_RejectsNonSequenceAndDictionaryShapes()
    {
        var converter = ReadAsArrayJsonConverter.Instance;

        Assert.False(converter.CanConvert(typeof(string)));
        Assert.False(converter.CanConvert(typeof(Dictionary<string, int>)));
        Assert.False(converter.CanConvert(typeof(int[,])));
        Assert.True(converter.CanConvert(typeof(Queue<int>)));
    }

    [Fact]
    public void ReadAndWrite_UsesDefaultCollectionConverter()
    {
        var options = JsonHelper.GetOptions().AddConverters([ReadAsArrayJsonConverter.Instance]);

        var value = JsonSerializer.Deserialize<Queue<int>>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(1, Assert.Single(value!));
        Assert.Equal("[1]", json);
    }

    [Fact]
    public void ReadAndWrite_ComposesNextCollectionConverter()
    {
        var options = JsonHelper.GetOptions().AddConverters([
            ReadAsArrayJsonConverter.Instance,
            QueueJsonConverter.Instance,
        ]);

        var value = JsonSerializer.Deserialize<Queue<int>>("1", options);
        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal(2, Assert.Single(value!));
        Assert.Equal("\"2\"", json);
    }

    [Fact]
    public void TypeLevelRegistration_IsRejectedInsteadOfRecursing()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => JsonSerializer.Deserialize<AttributedList>("1"));

        Assert.Contains("Apply ReadAsArrayJsonConverter to a property", exception.Message);
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
    private sealed class AttributedList : List<int>;
}
