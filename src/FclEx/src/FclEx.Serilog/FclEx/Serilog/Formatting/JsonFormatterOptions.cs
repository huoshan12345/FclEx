namespace FclEx.Serilog.Formatting;

public class JsonFormatterOptions
{
    public JsonValueFormatter ValueFormatter { get; set; } = new(null); // we don't need type tag as default
    public string UtcTimeName { get; set; } = "@t";
    public string MessageName { get; set; } = "@m";
    public string LogLevelName { get; set; } = "@l";
    public string ExceptionName { get; set; } = "@x";

    /// <summary>
    /// Indicates whether or not enable formatting for exception. <br/>
    /// <see langword="true" /> to format exception according to <see cref="ExceptionFormatOptions"/>, <br/>
    /// <see langword="false" /> to use <see cref="Exception.ToString()"/>.
    /// </summary>
    public bool EnableExceptionFormat { get; set; } = true;
    public ExceptionFormatOptions ExceptionFormatOptions { get; set; } = ExceptionFormatOptions.Default;

    /// <summary>
    /// Indicates how to print exception information to <see cref="Console"/>. <br/>
    /// It is useful if the Vector Agent is used to forward log messages from <see cref="Console"/>.
    /// </summary>
    public ExceptionPrintOptions ExceptionPrintOptions { get; set; } = ExceptionPrintOptions.DonotPrint;

    public static readonly JsonFormatterOptions Default = new();
}

public enum ExceptionPrintOptions
{
    DonotPrint,

    /// <summary>
    /// Print <see cref="Exception.ToString()"/> to <see cref="Console"/>.
    /// </summary>
    SingleMessage,

    /// <summary>
    /// Print each line of <see cref="Exception.ToString()"/> to <see cref="Console"/>.
    /// </summary>
    MessagesForEachLine,
}

[Flags]
public enum ExceptionWriteIndexOptions
{
    DonotWrite = 0,

    /// <summary>
    /// Write the indexes of exceptions to log message.
    /// </summary>
    Write = 1 << 0,

    /// <summary>
    /// Write the indexes of exceptions to log message only when there are more than one exceptions.
    /// </summary>
    WriteOnlyForMultiple = 1 << 1,

    /// <summary>
    /// Write the indexes of exceptions to log message only when the structure of exceptions is a multi-branched tree.
    /// </summary>
    WriteOnlyForMultiBranched = 1 << 2,

    Default = Write | WriteOnlyForMultiple | WriteOnlyForMultiBranched,
}

public class ExceptionFormatOptions
{
    public int? MaxMessageLength { get; set; } = 100;

    /// <summary>
    /// Indicates whether or not parameters will be skipped when a stack trace is printed.
    /// </summary>
    public bool SkipParasInStackTrace { get; set; } = true;

    /// <summary>
    /// Indicates whether or not exception message will be skipped when it already exists in rendered <see cref="MessageTemplate"/>
    /// </summary>
    public bool SkipMessageIfExists { get; set; } = true;

    /// <summary>
    /// Indicates how to print the name of an exception type. <br/>
    /// <see langword="true" /> to use <see cref="TypeExtensions.SimpleName"/>, <br/>
    /// <see langword="false" /> to use <see cref="Type.FullName"/>.
    /// </summary>
    public bool UseSimpleNameForType { get; set; } = true;

    public static readonly ExceptionFormatOptions Default = new();

    /// <summary>
    /// Indicates whether or not the indexes of an exception will be written.
    /// </summary>
    public ExceptionWriteIndexOptions WriteIndexOptions { get; set; } = ExceptionWriteIndexOptions.Default;
}

