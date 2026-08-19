namespace FclEx.Utils;

public class ConsoleTable : IRenderable
{
    private readonly object?[] _columns;
    public IReadOnlyList<object?> Columns { get; }

    private readonly List<object?[]> _rows = [];
    public IReadOnlyList<IReadOnlyList<object?>> Rows => _rows
        .Select(IReadOnlyList<object?> (m) => Array.AsReadOnly(m))
        .ToArray();

    public ConsoleTableOptions Options { get; }

    public ConsoleTable(ConsoleTableOptions options)
    {
        Options = options;
        _columns = options.Columns?.Cast<object?>().ToArray() ?? [];
        Columns = Array.AsReadOnly(_columns);
    }

    /// <summary>
    /// Adds a row whose number of cells exactly matches the configured columns.
    /// </summary>
    /// <param name="values">The row values.</param>
    /// <returns>This table.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The table has no configured columns.</exception>
    /// <exception cref="ArgumentException"><paramref name="values"/> has a different number of cells than the columns.</exception>
    public ConsoleTable AddRow(object?[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var len = Columns.Count;
        if (len == 0)
            throw new InvalidOperationException("Please set the columns first.");

        if (len != values.Length)
            throw new ArgumentException($"The number of columns ({len}) does not match the number of values ({values.Length}).", nameof(values));

        _rows.Add(values.ToArray());
        return this;
    }

    public void Render(StringBuilder builder)
    {
        // create the string format with padding
        var format = BuildFormat();

        // find the longest formatted line
        var maxRowLength = Math.Max(0, _rows.Any() ? _rows.Max(row => string.Format(format, args: row).Length) : 0);

        var columnHeaders = Options.RenderColumns ? string.Format(format, args: _columns) : "";
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

        foreach (var row in _rows.Select(row => string.Format(format, row)))
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
            using var sb = new ValueStringBuilder(1024);

            for (var i = 0; i < Columns.Count; i++)
            {
                // find the longest column by searching each row
                var len = GetColumnLength(i);
                sb.Append(" | {");
                sb.Append(i.ToString());
                sb.Append(',');
                sb.Append('-');
                sb.Append(len.ToString());
                sb.Append('}');
            }
            sb.Append(" |");

            return sb.ToString();
        }

        int GetColumnLength(int index)
        {
            var column = Options.RenderColumns ? Columns[index] : null;
            var row = _rows.Select(m => m[index]);
            return row.Append(column).Max(m => m?.ToString()?.Length) ?? 0;
        }
    }

    public override string ToString()
    {
        return StringBuilder.Build(Render);
    }
}
