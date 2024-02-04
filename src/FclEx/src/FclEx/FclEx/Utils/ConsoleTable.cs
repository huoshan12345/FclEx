namespace FclEx.Utils;

public class ConsoleTable
{
    public object?[] Columns { get; }
    public List<object?[]> Rows { get; }
    public ConsoleTableOptions Options { get; }

    public ConsoleTable(ConsoleTableOptions options)
    {
        Options = options;
        Rows = new List<object?[]>();
        Columns = options.Columns.EmptyIfNull().AsArray();
    }

    public void Render(StringBuilder builder)
    {
        // create the string format with padding
        var format = BuildFormat();

        // find the longest formatted line
        var maxRowLength = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, args: row).Length) : 0);

        var columnHeaders = Options.RenderColumns ? string.Format(format, args: Columns) : "";
        // longest line is greater of formatted columnHeader and longest row
        var longestLine = Math.Max(maxRowLength, columnHeaders.Length);

        if (Options.Title is { Length: > 0 } title)
        {
            var spaceCount = (longestLine - 1 - title.Length) / 2;
            for (var i = 0; i < spaceCount; i++)
            {
                builder.Append(' ');
            }
            builder.AppendLine(title);
        }

        if (Options.RenderColumns)
        {
            RenderDivider(builder, longestLine);
            builder.AppendLine(columnHeaders);
        }

        foreach (var row in Rows.Select(row => string.Format(format, row)))
        {
            RenderDivider(builder, longestLine);
            builder.AppendLine(row);
        }
        RenderDivider(builder, longestLine);

        static void RenderDivider(StringBuilder builder, int len)
        {
            builder.Append(' ');
            for (var i = 0; i < len - 1; i++)
            {
                builder.Append('-');
            }
            builder.Append(' ');
            builder.AppendLine();
        }

        string BuildFormat()
        {
            using var builder = new ValueStringBuilder(1024);

            for (var i = 0; i < Columns.Length; i++)
            {
                // find the longest column by searching each row
                var len = GetColumnLength(i);
                builder.Append(" | {");
                builder.Append(i.ToString());
                builder.Append(',');
                builder.Append('-');
                builder.Append(len.ToString());
                builder.Append('}');
            }
            builder.Append(" |");

            return builder.ToString();
        }

        int GetColumnLength(int index)
        {
            var column = Options.RenderColumns ? Columns[index] : null;
            var row = Rows.Select(m => m[index]);
            return row.Append(column).Max(m => m?.ToString()?.Length) ?? 0;
        }
    }
}

public static class ConsoleTableExtensions
{
    public static ConsoleTable AddRow(this ConsoleTable table, string?[] values)
    {
        // ReSharper disable once CoVariantArrayConversion
        return table.AddRow((object?[])values); // NOTE: string[] to object[] is OK;
    }

    public static ConsoleTable AddRow(this ConsoleTable table, object?[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var len = table.Columns.Length;
        if (len == 0)
            throw new Exception("Please set the columns first");

        if (len != values.Length)
            throw new Exception($"The number columns in the row ({len}) does not match the values ({values.Length})");

        table.Rows.Add(values);
        return table;
    }
}

public readonly record struct ConsoleTableOptions(IEnumerable<object>? Columns = null, bool RenderColumns = false, string? Title = null);