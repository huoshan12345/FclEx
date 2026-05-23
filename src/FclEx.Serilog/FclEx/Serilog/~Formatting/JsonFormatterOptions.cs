namespace FclEx.Serilog;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class JsonFormatterOptions
{
    public JsonValueFormatter ValueFormatter { get; set; } = new(null); // we don't need type tag as default
    public string UtcTimeName { get; set; } = "@t";
    public string MessageName { get; set; } = "@m";
    public string LogLevelName { get; set; } = "@l";
    public string ExceptionName { get; set; } = "@x";

    /// <summary>
    /// Indicates whether enable formatting for exception. <br/>
    /// <see langword="true" /> to format exception according to <see cref="ExceptionFormatOptions"/>, <br/>
    /// <see langword="false" /> to use <see cref="Exception.ToString()"/>.
    /// </summary>
    public bool EnableExceptionFormat { get; set; } = true;
    public ExceptionFormatOptions ExceptionFormatOptions { get; set; } = ExceptionFormatOptions.Default;

    public static readonly JsonFormatterOptions Default = new();
}