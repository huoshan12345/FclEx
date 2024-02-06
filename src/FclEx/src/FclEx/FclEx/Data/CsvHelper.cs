namespace FclEx.Data;

public static class CsvHelper
{
    private static string ConvertToCsvCell(this object? obj)
    {
        if (obj == null) return "";
        var value = obj.ToString() ?? string.Empty;

        var mustQuote = value.Any(x => x is ',' or '\"' or '\r' or '\n');
        if (!mustQuote)
        {
            return value;
        }
        value = value.Replace("\"", "\"\"");
        return $"\"{value}\"";
    }

    public static byte[] ToCsvBytes<T>(IEnumerable<T> dataSource) where T : ExportModel<T>, new()
    {
        return ToCsvBytes(dataSource, ExportModel<T>.Columns);
    }

    public static byte[] ToCsvBytes<T>(IEnumerable<T> dataSource, IList<IExportColumn<T>> columns)
    {
        using var mem = new MemoryStream();
        using var sw = new StreamWriter(mem, Encoding.UTF8);

        var title = columns.Select(m => m.Title).JoinWith(",");
        sw.WriteLine(title);
        foreach (var (item, index) in dataSource.Select((m, i) => (m, i)))
        {
            var str = columns.Select(m => m.GetValue(item, index).ConvertToCsvCell()).JoinWith(",");
            sw.WriteLine(str);
        }
        sw.Flush();
        return mem.ToArray();
    }
}