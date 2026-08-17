namespace FclEx.Utils;

public static class ConsoleTableExtensions
{
    public static ConsoleTable AddRow(this ConsoleTable table, string?[] values)
    {
        // ReSharper disable once CoVariantArrayConversion
        return table.AddRow((object?[])values); // NOTE: string[] to object[] is OK;
    }
}