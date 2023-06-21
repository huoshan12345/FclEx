using FclEx.Extensions;
using FclEx.Json.Converters;
using Newtonsoft.Json;

namespace FclEx.Json;

public class ReadAsStringConverterTests
{
    private class Tester
    {
        [JsonConverter(typeof(ReadAsStringConverter))]
        public string? MatchId { get; set; }
        [JsonConverter(typeof(ReadAsStringConverter))]
        public string? Grades { get; set; }
    }

    private class GradeItem
    {
        public string? Grade { get; set; }
        public string? LessonId { get; set; }
    }

    [Fact]
    public void Test()
    {
        var json = "{\"matchId\":11,\"grades\":[{\"grade\":\"1\",\"lessonId\":\"123\"}]}";
        var obj = json.ToJToken().ToObject<Tester>();
        Assert.NotNull(obj);
        Assert.NotNull(obj.Grades);

        var grades = obj.Grades.ToJToken().ToObject<GradeItem[]>();
        Assert.Equal("11", obj.MatchId);
        Assert.Single(grades.EmptyIfNull());
    }
}