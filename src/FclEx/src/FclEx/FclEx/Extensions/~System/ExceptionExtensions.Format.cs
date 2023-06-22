using FclEx.Helpers;

namespace FclEx.Extensions;

// NOTE: StackTraceLines will be from the outermost stack trace frame to the innermost, which is opposite from Exception.StackTrace.
public readonly record struct ExceptionIofo(Type Type, string Message, IReadOnlyList<string> StackTraceLines, int Index, int ParentIndex);

public readonly record struct ExceptionIofos(IReadOnlyList<ExceptionIofo> Infos, bool MultiBranched);

partial class ExceptionExtensions
{
    // NOTE: Infos will be from the outermost exception to the innermost.
    public static ExceptionIofos GetInfos(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var list = new List<ExceptionIofo>();
        var queue = new Queue<(Exception, int ParentIndex)>();
        queue.Enqueue((exception, -1));
        var index = 0;
        var multiBranched = false;
        while (queue.Count != 0)
        {
            var (ex, parent) = queue.Dequeue();
            if (ex is AggregateException { InnerExceptions: { } inners } aggEx)
            {
                if (inners.Count == 1)
                {
                    var msg = aggEx.GetMessage();
                    if (msg.IsNullOrEmpty() || msg.Equals("One or more errors occurred."))
                    {
                        queue.Enqueue((inners[0], index));
                        continue;
                    }
                }

                var info = GetInfo(ex, ref index, parent);
                list.Add(info);

                foreach (var inner in inners)
                {
                    queue.Enqueue((inner, info.Index));
                }

                if (multiBranched == false && inners.Count > 1)
                    multiBranched = true;
            }
            else
            {
                var info = GetInfo(ex, ref index, parent);
                list.Add(info);

                if (ex.InnerException is { } inner)
                {
                    queue.Enqueue((inner, info.Index));
                }
            }
        }
        return new(list, multiBranched);
    }

    internal static ExceptionIofo GetInfo(this Exception exception, ref int index, int parentIndex)
    {
        var lines = exception.StackTrace is not { } trace
            ? Array.Empty<string>()
            : trace
                .SplitToLines()
                .Select(m => m.TrimStart("   at ").TrimStart()) // see https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Diagnostics/StackTrace.cs,226
                .Where(m => IgnorableStackTracePrefixes.All(x => m.StartsWith(x) == false))
                .Reverse() // reverse frames to be more readable (from the outermost to the innermost).
                .ToArray();

        var info = new ExceptionIofo(exception.GetType(), exception.Message, lines, index++, parentIndex);
        return info;
    }

    public static string[] IgnorableStackTracePrefixes { get; } =
    {
        "--- End of ", // --- End of inner exception stack trace --- or --- End of stack trace from previous location ---
        "System.Threading.ExecutionContext.",
        "System.Runtime.CompilerServices.AsyncTaskMethodBuilder",
        "System.Threading.Tasks.Task.",
        "System.Threading.Tasks.TaskCompletionSourceWithCancellation",
        "System.Threading.Tasks.AwaitTaskContinuation.",
        "Polly.Retry.AsyncRetryEngine.ImplementationAsync",
        "Polly.AsyncPolicy.<>c__DisplayClass",
        "System.Runtime.CompilerServices.AsyncMethodBuilderCore",
        "System.Threading.ThreadPoolWorkQueue.Dispatch",
        "System.Threading.PortableThreadPool.WorkerThread.WorkerThreadStart",
    };

    public static string ToFormattedString(this Exception exception)
    {
        var (infos, _) = exception.GetInfos();

        return StringBuilderHelper.Build(m =>
        {
            foreach (var info in infos)
            {
                var (type, message, lines, index, parentIndex) = info;
                if (infos.Count > 1)
                {
                    m.Append('[');
                    m.Append(index);
                    if (parentIndex >= 0)
                    {
                        m.Append("->");
                        m.Append(parentIndex);
                    }
                    m.Append(']');
                }

                m.Append(type.SimpleName());
                m.Append(": ");
                m.AppendLine(message);

                foreach (var line in lines)
                {
                    m.AppendLine(line);
                }
            }
        });

    }
}