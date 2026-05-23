using static FclEx.Serilog.ExceptionIndexOptions;

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

        output.Write(name); // we assume "name" doesn't need to escape. 

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
        var indexOp = ExceptionOptions.IndexOptions;
        var writeIndexes = IfWriteIndexes();

        foreach (var (_, info, isFirst, _) in infos.IndexEx())
        {
            if (isFirst == false)
                output.Write(",");

            WriteExceptionInfo(logEventMessage, info, output, writeIndexes);
        }
        output.Write(']');

        bool IfWriteIndexes()
        {
            if (indexOp.IsSet(Include) == false)
                return false;

            if (indexOp.IsSet(IncludeForMultiple) && infos.Count <= 1)
                return false;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (indexOp.IsSet(IncludeForMultiBranched) && multiBranched == false)
                return false;

            return true;
        }
    }
}