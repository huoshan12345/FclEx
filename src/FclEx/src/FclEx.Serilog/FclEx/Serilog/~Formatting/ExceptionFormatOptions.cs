namespace FclEx.Serilog;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class ExceptionFormatOptions
{
    public int? MaxMessageLength { get; set; } = 100;

    /// <summary>
    /// Indicates whether parameters will be skipped when a stack trace is printed.
    /// </summary>
    public bool SkipParasInStackTrace { get; set; } = true;

    /// <summary>
    /// Indicates whether exception message will be skipped when it already exists in rendered <see cref="MessageTemplate"/>
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
    /// Indicates whether the indexes of an exception will be written.
    /// </summary>
    public ExceptionWriteIndexOptions WriteIndexOptions { get; set; } = ExceptionWriteIndexOptions.Default;
}