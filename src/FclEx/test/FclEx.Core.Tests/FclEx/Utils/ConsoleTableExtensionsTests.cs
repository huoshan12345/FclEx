namespace FclEx.Utils;

public class ConsoleTableExtensionsTests
{
    private readonly ITestOutputHelper _output;

    public ConsoleTableExtensionsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Render_Test()
    {
        var columns = new[] { "LongLongLongLong", "Sh" };
        var rows = new string[][]
        {
            ["SuccessCount", "1762"],
            ["FailedCount", "22"],
            ["InvalidCount", "85"],
            ["TotalCount", "1869"],
        };
        var table = new ConsoleTable(new(columns));

        foreach (var row in rows)
        {
            table.AddRow(row);
        }

        var str = table.ToString();
        _output.WriteLine(str);
    }
}