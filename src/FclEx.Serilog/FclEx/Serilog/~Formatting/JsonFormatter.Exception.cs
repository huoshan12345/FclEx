namespace FclEx.Serilog;

partial class JsonFormatter
{
    /// <summary>The pattern used to remove the first parenthesized parameter list from a stack trace line.</summary>
    protected static readonly Regex ParametersPattern = new(@"\([^(]*\)(?=\s|$|\+)", RegexOptions.Compiled);

    /// <summary>Writes one exception description followed by its stack-frame strings as comma-separated JSON array elements.</summary>
    /// <param name="logEventMessage">The rendered message checked for duplicate exception text before truncation.</param>
    /// <param name="info">The exception type, message, frames, and retained tree indexes.</param>
    /// <param name="output">Receives quoted elements without surrounding array brackets.</param>
    /// <param name="writeIndexes">Whether the description begins with its index and optional parent index.</param>
    protected virtual void WriteExceptionInfo(string logEventMessage, ExceptionInfo info, TextWriter output, bool writeIndexes)
    {
        var (type, message, lines, index, parentIndex) = info;

        output.Write("\"");

        if (writeIndexes)
        {
            output.Write("[");
            output.Write(index);
            if (parentIndex >= 0)
            {
                output.Write("->");
                output.Write(parentIndex);
            }
            output.Write("]");
        }

        var name = ExceptionOptions.UseSimpleTypeName
            ? type.SimpleName()
            : type.FullName ?? type.Name;

        WriteJsonString(name, output);

        if (ExceptionOptions.OmitMessageIfExists == false
            || ExceptionOptions.OmitMessageIfExists && logEventMessage.Contains(message) == false)
        {
            var msg = ExceptionOptions.MaxMessageLength is { } maxMsg
                ? message.Truncate(maxMsg)
                : message;

            output.Write(": ");
            WriteJsonString(msg, output); // message need to escape. 
        }

        output.Write('"');

        foreach (var line in lines)
        {
            var l = ExceptionOptions.OmitParametersInStackTrace
                ? ParametersPattern.Replace(line, "", 1)
                : line;

            output.Write(",");
            JsonValueFormatter.WriteQuotedJsonString(l, output);
        }
    }

    /// <summary>Writes a comma-prefixed exception member containing flattened exception descriptions and stack-frame strings.</summary>
    /// <param name="logEventMessage">The rendered message used to omit duplicate exception messages.</param>
    /// <param name="ex">The root exception whose breadth-first tree is formatted.</param>
    /// <param name="output">Receives the complete exception array member.</param>
    /// <remarks>IndexPolicy applies to the retained exception tree after redundant aggregate wrappers are omitted.</remarks>
    protected virtual void WriteFormattedException(string logEventMessage, Exception ex, TextWriter output)
    {
        output.Write(',');
        JsonValueFormatter.WriteQuotedJsonString(Options.ExceptionName, output);
        output.Write(":[");

        var (infos, multiBranched) = ex.BuildTree();
        var writeIndexes = ExceptionOptions.IndexPolicy switch
        {
            ExceptionIndexPolicy.None => false,
            ExceptionIndexPolicy.Always => true,
            ExceptionIndexPolicy.WhenMultiple => infos.Count > 1,
            ExceptionIndexPolicy.WhenBranched => multiBranched,
            _ => throw new ArgumentOutOfRangeException(nameof(ExceptionFormatOptions.IndexPolicy), ExceptionOptions.IndexPolicy, "Unknown exception index policy.")
        };

        foreach (var (_, info, isFirst, _) in infos.IndexEx())
        {
            if (isFirst == false)
                output.Write(",");

            WriteExceptionInfo(logEventMessage, info, output, writeIndexes);
        }
        output.Write(']');
    }
}
