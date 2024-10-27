namespace FclEx.Utils;

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