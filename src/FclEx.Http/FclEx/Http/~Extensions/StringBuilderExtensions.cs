namespace FclEx.Http;

/// <summary>
/// Extensions for writing HTTP-style text lines.
/// </summary>
public static class StringBuilderExtensions
{
    /// <summary>
    /// Appends a value followed by the HTTP CRLF newline sequence.
    /// </summary>
    public static StringBuilder AppendHttpLine(this StringBuilder sb, string value)
    {
        return sb.Append(value + HttpConstants.NewLine);
    }
}
