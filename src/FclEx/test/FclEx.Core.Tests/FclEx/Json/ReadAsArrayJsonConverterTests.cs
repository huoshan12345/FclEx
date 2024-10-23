namespace FclEx.Json;

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
    public void Read_FromArray_Test()
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
    public void Read_FromSingleValue_Test()
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
}