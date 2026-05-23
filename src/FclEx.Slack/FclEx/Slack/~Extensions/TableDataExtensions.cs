using SlackNet.WebApi;

namespace FclEx.Slack;

public static class TableDataExtensions
{
    public static Message ToSlackMessage(this TableData tableData)
    {
        var (title, tableTitle, columns, rows, renderColumns) = tableData;
        var table = new ConsoleTable(new ConsoleTableOptions { Columns = columns, RenderColumns = renderColumns, Title = tableTitle });
        foreach (var row in rows)
        {
            table.AddRow(row);
        }

        var text = SlackStringBuilder.Build(m => m.AppendCodeBlock(x => table.Render(x.Builder)));
        var message = new Message()
            .AddMarkdown(title)
            .AddMarkdown(text);

        return message;
    }

    public static TableData WithTableTitle(this TableData table, int? index, int skip, int take)
    {
        if (index is not { } i)
            return table;

        var suffix = $"(Part {i + 1})";
        var newTitle = table.TableTitle is { Length: > 0 } tableTitle
            ? tableTitle + " " + suffix
            : suffix;
        return table with { TableTitle = newTitle, Rows = table.Rows.Skip(skip).Take(take) };
    }
}