namespace FclEx.Serilog;

partial class JsonFormatter
{
    protected static readonly Regex ParametersPattern = new(@"\([^(]*\)(?=\s|$|\+)", RegexOptions.Compiled);

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
