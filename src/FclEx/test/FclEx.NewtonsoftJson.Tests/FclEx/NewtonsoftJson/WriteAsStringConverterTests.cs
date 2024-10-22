using FclEx.Comparers;
using Newtonsoft.Json.Linq;

namespace FclEx.NewtonsoftJson;

public class WriteAsStringConverterTests
{
    private class Tester
    {
        [JsonConverter(typeof(WriteAsStringConverter))]
        public int MatchId { get; set; }
        [JsonConverter(typeof(WriteAsStringConverter))]
        public GradeItem[]? Grades { get; set; }
    }

    private class GradeItem
    {
        public string? Grade { get; set; }
        public string? LessonId { get; set; }
    }

    [Fact]
    public void Test()
    {
        var obj = new Tester
        {
            MatchId = 11,
            Grades = new[]
            {
                new GradeItem
                {
                    Grade = "1",
                    LessonId = "123"
                },
            }
        };

        var json = obj.ToNewtonsoftJson(useCamelCase: true);

        var tokenOfMatchId = json.ToJToken()["matchId"];
        Assert.NotNull(tokenOfMatchId);
        Assert.Equal(JTokenType.String, tokenOfMatchId.Type);

        var tokenOfGrades = json.ToJToken()["grades"];
        Assert.NotNull(tokenOfGrades);
        Assert.Equal(JTokenType.String, tokenOfGrades.Type);

        var tokenUnwrapped = tokenOfGrades.ToString().ToJToken();
        Assert.NotNull(tokenUnwrapped);
        Assert.Equal(JTokenType.Array, tokenUnwrapped.Type);

        var grades = tokenUnwrapped.ToObject<GradeItem[]>()!;

        Assert.Equal(obj.Grades, grades, KeyEqualityComparer<GradeItem>.Create(m => (m?.LessonId, m?.Grade)));
    }
}