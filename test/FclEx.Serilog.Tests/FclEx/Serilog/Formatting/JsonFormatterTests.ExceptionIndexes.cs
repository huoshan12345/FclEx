using System.Text.Json;

namespace FclEx.Serilog.Formatting;

public partial class JsonFormatterTests
{
    public static TheoryData<ExceptionIndexPolicy, int, bool> ExceptionIndexCases =>
        (from policy in Enum.GetValues(typeof(ExceptionIndexPolicy)).Cast<ExceptionIndexPolicy>()
         from shape in Enumerable.Range(0, 4)
         let expected = policy == ExceptionIndexPolicy.Always
                        || policy == ExceptionIndexPolicy.WhenMultiple && shape is 1 or 2
                        || policy == ExceptionIndexPolicy.WhenBranched && shape == 2
         select (policy, shape, expected)).ToTheoryData();

    [Theory]
    [MemberData(nameof(ExceptionIndexCases))]
    public void Format_ExceptionIndexPolicy_MatchesTreeShape(ExceptionIndexPolicy policy, int shape, bool expected)
    {
        Exception exception = shape switch
        {
            0 => new InvalidOperationException("single"),
            1 => new InvalidOperationException("outer", new ArgumentException("inner")),
            2 => new AggregateException("branches", new InvalidOperationException("first"), new ArgumentException("second")),
            // BuildTree removes the default AggregateException wrapper when it has only one child.
            _ => new AggregateException(new InvalidOperationException("single"))
        };
        var formatter = new JsonFormatter(new JsonFormatterOptions
        {
            ExceptionFormatOptions = new ExceptionFormatOptions { IndexPolicy = policy }
        });
        using var output = new StringWriter();

        formatter.Format(SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception, "message"), output);

        using var document = JsonDocument.Parse(output.ToString());
        var lines = document.RootElement.GetProperty("@x").EnumerateArray().Select(line => line.GetString()!).ToArray();
        Assert.All(lines, line => Assert.Equal(expected, line.StartsWith("[", StringComparison.Ordinal)));
        if (expected && shape is 1 or 2)
        {
            Assert.StartsWith("[0]", lines[0]);
            Assert.StartsWith("[1->0]", lines[1]);
            if (shape == 2)
                Assert.StartsWith("[2->0]", lines[2]);
        }
    }

    [Fact]
    public void ExceptionFormatOptions_DefaultIndexesOnlyBranchedTrees()
    {
        Assert.Equal(ExceptionIndexPolicy.WhenBranched, new ExceptionFormatOptions().IndexPolicy);
    }
}
