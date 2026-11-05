namespace FclEx.Serilog;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class JsonFormatterOptions
{
    public JsonValueFormatter ValueFormatter { get; set; } = new(null); // we don't need type tag as default
    /// <summary>The UTC timestamp member name, defaulting to @t. An empty name omits the timestamp.</summary>
    public string UtcTimeName { get; set; } = "@t";
    /// <summary>The rendered message member name, defaulting to @m. An empty name omits the message.</summary>
    public string MessageName { get; set; } = "@m";
    /// <summary>The event level member name, defaulting to @l. An empty name omits the level.</summary>
    public string LogLevelName { get; set; } = "@l";
    /// <summary>The exception member name, defaulting to @x. An empty name omits exception formatting and output.</summary>
    public string ExceptionName { get; set; } = "@x";
    public JsonNamingPolicy PropertyNamingPolicy { get; set; } = JsonNamingPolicy.CamelCase;

    /// <summary>
    /// Indicates whether enable formatting for exception. <br/>
    /// <see langword="true" /> to format exception according to <see cref="ExceptionFormatOptions"/>, <br/>
    /// <see langword="false" /> to use <see cref="Exception.ToString()"/>.
    /// </summary>
    public bool EnableExceptionFormat { get; set; } = true;
    public ExceptionFormatOptions ExceptionFormatOptions { get; set; } = ExceptionFormatOptions.Default;

    public static readonly JsonFormatterOptions Default = new();
}
