using static FclEx.Serilog.ExceptionPrintOption;
using static FclEx.Serilog.ExceptionWriteIndexOptions;

namespace FclEx.Serilog;

partial class JsonFormatter
{
    protected static readonly Regex RegexOfParas = new(@"\([^(]*\)(?=\s|$)", RegexOptions.Compiled);

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

        var name = ExceptionOptions.UseSimpleNameForType
            ? type.SimpleName()
            : type.FullName;

        output.Write(name); // we assume "name" doesn't need to escape. 

        if (ExceptionOptions.SkipMessageIfExists == false
            || ExceptionOptions.SkipMessageIfExists && logEventMessage.Contains(message) == false)
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
            var l = ExceptionOptions.SkipParasInStackTrace
                ? RegexOfParas.Replace(line, "", 1)
                : line;

            output.Write(",");
            JsonValueFormatter.WriteQuotedJsonString(l, output);
        }
    }

    protected virtual void PrintException(Exception ex)
    {
        var op = Options.ExceptionPrintOption;
        if (op == ExceptionPrintOption.None)
            return;

        var str = ex.ToString();
        var lines = op == SingleMessage
            ? [str]
            : str.SplitToLines();

        foreach (var line in ToJsonObject(lines))
        {
            Console.WriteLine(line);
        }

        IEnumerable<string> ToJsonObject(IEnumerable<string> values)
        {
            using var sb = ObjectPoolHelper.StringBuilderPool.GetPooled();
            var sw = new StringWriter(sb.Value);

            foreach (var value in values)
            {
                sb.Value.Clear();
                sw.Write("{");
                WriteJsonData(Options.ExceptionName, value, sw, false);
                sw.Write("}");
                yield return sw.ToString();
            }
        }
    }

    protected virtual void WriteFormattedException(string logEventMessage, Exception ex, TextWriter output)
    {
        output.Write(',');
        JsonValueFormatter.WriteQuotedJsonString(Options.ExceptionName, output);
        output.Write(":[");

        var (infos, multiBranched) = ex.GetInfos();
        var indexOp = ExceptionOptions.WriteIndexOptions;
        var writeIndexes = IfWriteIndexes();

        foreach (var (_, info, isFirst, _) in infos.IndexExt())
        {
            if (isFirst == false)
                output.Write(",");

            WriteExceptionInfo(logEventMessage, info, output, writeIndexes);
        }
        output.Write(']');

        bool IfWriteIndexes()
        {
            if (indexOp.IsSet(Write) == false)
                return false;

            if (indexOp.IsSet(WriteOnlyForMultiple) && infos.Count <= 1)
                return false;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (indexOp.IsSet(WriteOnlyForMultiBranched) && multiBranched == false)
                return false;

            return true;
        }
    }
}