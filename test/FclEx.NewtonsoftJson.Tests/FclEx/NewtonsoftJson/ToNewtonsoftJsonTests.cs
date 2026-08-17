namespace FclEx.NewtonsoftJson;

public class ToNewtonsoftJsonTests
{
    internal class DateTimeTestModel
    {
        public string? Name { get; set; }
        public DateTime DateTime { get; set; }
    }

    internal class TestModel
    {
        public string Name { get; set; } = "Name";
        [JsonProperty(nameof(Count))]
        public int Count { get; set; } = 1;
    }

    [Fact]
    public void ToJsonCamel_Test()
    {
        var obj = new TestModel();
        var json = obj.ToNewtonsoftJsonCamelCase();
        Assert.Equal("{\"name\":\"Name\",\"Count\":1}", json);
    }

    [Fact]
    public void DateTimeToJsonCamel_Test()
    {
        foreach (var kind in Enum.GetValues<DateTimeKind>())
        {
            var obj = new DateTimeTestModel() { DateTime = new DateTime(2019, 1, 2, 3, 4, 5, kind) };
            var json = obj.ToNewtonsoftJsonCamelCase();
            var obj2 = json.ToJToken().ToObject<DateTimeTestModel>()!;
            Assert.Equal(obj.Name, obj2.Name);
            Assert.Equal(obj.DateTime.AssumeUtc(), obj2.DateTime.AssumeUtc());
        }

    }

    [Fact]
    public void GetSettings_SameOptions_SameResult()
    {
        var options = new NewtonsoftJsonOptions();
        var settings = NewtonsoftJsonHelper.GetOptions(options);
        var settings2 = NewtonsoftJsonHelper.GetOptions(options);
        Assert.Same(settings, settings2);
    }

    [Fact]
    public void GetSettings_EquatableOptions_SameResult()
    {
        var settings = NewtonsoftJsonHelper.GetOptions(new NewtonsoftJsonOptions(Formatting.Indented));
        var settings2 = NewtonsoftJsonHelper.GetOptions(new NewtonsoftJsonOptions(Formatting.Indented));
        Assert.Same(settings, settings2);
    }

    [Fact]
    public void GetSettings_NonEquatableOptions_DifferentResult()
    {
        var settings = NewtonsoftJsonHelper.GetOptions(new NewtonsoftJsonOptions(Formatting.Indented));
        var settings2 = NewtonsoftJsonHelper.GetOptions(new NewtonsoftJsonOptions(Formatting.None));
        Assert.NotSame(settings, settings2);
    }
}
