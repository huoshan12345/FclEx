namespace Json.Path.Tests;

public class GithubTests
{
    [Theory]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==10)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==11)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==12)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==13)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==14)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==15)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==16)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==17)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==18)].doc_count")]
    [InlineData("$.aggregations.data.buckets[0].grade.buckets[?(@.key==19)].doc_count")]
    public void Issue150_ParseFailsWhenExpressionContainsLiteralNumberWith9(string pathText)
    {
        var path = JsonPath.Parse(pathText);
        Console.WriteLine(path);
    }

    [Fact]
    public void Issue463_StringComparison()
    {
        var data = new JsonArray
        (
            "2023-04-23",
            "2023-06-07",
            "2023-07-08",
            "2023-08-09",
            "2023-09-10",
            "2024-01-01"
        );

        var path = JsonPath.Parse("$[?@ >= '2023-05-01']");
        var results = path.Evaluate(data);

        Assert.Equal(5, results.Matches!.Count);
    }

    [Fact]
    public void Issue495_TryParseEmptyString()
    {
        var success = JsonPath.TryParse(string.Empty, out var path);

        Assert.Multiple(() =>
        {
            Assert.False(success);
            Assert.Null(path);
        });
    }

    [Fact]
    public void Issue787_DotSyntaxWithoutNameThrowsInTryParse()
    {
        var pathText = "$.";

        Assert.False(JsonPath.TryParse(pathText, out _));
    }

    [Fact]
    public void Issue980_PathOnRightOfExpressionOperator()
    {
        var jsonDocument = new JsonObject
        {
            ["data"] = new JsonArray
            {
                new JsonObject
                {
                    ["elements"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["name"] = "ID",
                            ["value"] = "TestValue"
                        }
                    }
                }
            },
            ["name"] = new JsonObject
            {
                ["id"] = "ID"
            }
        };

        var expectedResult = new JsonArray
        {
            new JsonObject
            {
                ["elements"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["name"] = "ID",
                        ["value"] = "TestValue"
                    }
                }
            }
        };

        var path = JsonPath.Parse("$.data[?@.elements[?@.name == $.name.id]]");
        var result = path.Evaluate(jsonDocument);
        var matches = new JsonArray([.. from match in result.Matches select match.Value?.DeepClone()]);
        Assert.Equal(matches.ToJsonString(), expectedResult.ToJsonString());
    }

}
