using FclEx.Json;
using FclEx.NewtonsoftJson;

namespace FclEx.Extensions.JsonExtensions;

public class ToJsonTests
{
    [Fact]
    public void ToJsonCamel_Test()
    {
        var obj = new Tester();
        var json = obj.ToNewtonsoftJsonCamelCase();
        Assert.Equal("{\"name\":\"Name\",\"Count\":1}", json);
    }

    [Fact]
    public void DateTimeToJsonCamel_Test()
    {
        foreach (var kind in Enum.GetValues<DateTimeKind>())
        {
            var obj = new DateTimeTester() { DateTime = new DateTime(2019, 1, 2, 3, 4, 5, kind) };
            var json = obj.ToNewtonsoftJsonCamelCase();
            var obj2 = json.ToJToken().ToObject<DateTimeTester>()!;
            Assert.Equal(obj.Name, obj2.Name);
            Assert.Equal(obj.DateTime.ToUtc(), obj2.DateTime.ToUtc());
        }

    }

    [Fact]
    public void GetSettings_SameOptions_SameResult()
    {
        var options = new JsonOptions();
        var settings = JsonHelper.GetOptions(options);
        var settings2 = JsonHelper.GetOptions(options);
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