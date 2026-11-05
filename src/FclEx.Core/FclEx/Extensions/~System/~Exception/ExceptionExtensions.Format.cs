namespace FclEx.Extensions;

/// <summary>
/// Describes one retained exception node and its filtered stack trace, ordered from outermost to innermost frame.
/// </summary>
/// <param name="Type">The type of the exception.</param>
/// <param name="Message">The message associated with the exception.</param>
/// <param name="StackTraceLines">The stack trace lines, starting from the outermost frame.</param>
/// <param name="Index">The zero-based index of this retained node in the exception tree.</param>
/// <param name="ParentIndex">The index of its retained parent, or -1 when this node is the root after redundant wrappers are omitted.</param>
public readonly record struct ExceptionInfo(Type Type, string Message, IReadOnlyList<string> StackTraceLines, int Index, int ParentIndex);

/// <summary>
/// Represents a collection of exception information, potentially forming a tree structure.
/// </summary>
/// <param name="Nodes">A list of exception details (nodes) in the tree.</param>
/// <param name="MultiBranched">Indicates if the exceptions form a multi-branched structure.</param>
public readonly record struct ExceptionTree(IReadOnlyList<ExceptionInfo> Nodes, bool MultiBranched);

partial class ExceptionExtensions
{
    public static IReadOnlyCollection<string> ExcludeStackTracePrefixes { get; } =
    [
        "--- End of ", // --- End of inner exception stack trace --- or --- End of stack trace from previous location ---
        "System.Threading.ExecutionContext.",
        "System.Runtime.CompilerServices.AsyncTaskMethodBuilder",
        "System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder",
        "System.Threading.Tasks.Task.",
        "System.Threading.Tasks.Task`1.",
        "System.Threading.Tasks.TaskCompletionSourceWithCancellation",
        "System.Threading.Tasks.AwaitTaskContinuation.",
        "Polly.Retry.AsyncRetryEngine.ImplementationAsync",
        "Polly.AsyncPolicy.<>c__DisplayClass",
        "Polly.AsyncPolicy`1.ExecuteAsync",
        "Polly.Timeout.AsyncTimeoutEngine.ImplementationAsync",
        "System.Runtime.CompilerServices.AsyncMethodBuilderCore",
        "System.Threading.ThreadPoolWorkQueue.Dispatch",
        "System.Threading.PortableThreadPool.WorkerThread.WorkerThreadStart",
        "System.Threading.Tasks.ContinuationResultTaskFromResultTask",
    ];

    public static Func<string, bool> DefaultStackTraceLineFilter { get; } = m => ExcludeStackTracePrefixes.Any(m.StartsWith);

    /// <summary>
    /// Builds a breadth-first tree of exception details, omitting redundant single-child AggregateException wrappers.
    /// </summary>
    /// <param name="exception">The root exception to start building the tree from.</param>
    /// <param name="stackTraceLineFilter">Returns true for stack trace lines to exclude, or null to use DefaultStackTraceLineFilter.</param>
    /// <returns>
    /// Retained nodes with zero-based indexes and parent indexes, plus whether any aggregate has multiple children.
    /// </returns>
    /// <remarks>
    /// An aggregate with one child is omitted when its own message is empty or the default English message.
    /// Its child inherits the omitted wrapper's parent; a retained root has ParentIndex equal to -1.
    /// Stack trace lines are filtered and ordered from outermost to innermost frame.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static ExceptionTree BuildTree(this Exception exception, Func<string, bool>? stackTraceLineFilter = null)
    {
        Check.NotNull(exception);

        var list = new List<ExceptionInfo>();
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
                        // No node is added for this wrapper, so retain its existing parent relationship.
                        queue.Enqueue((inners[0], parent));
                        continue;
                    }
                }

                var info = ex.GetInfo(ref index, parent, stackTraceLineFilter);
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
                var info = ex.GetInfo(ref index, parent, stackTraceLineFilter);
                list.Add(info);

                if (ex.InnerException is { } inner)
                {
                    queue.Enqueue((inner, info.Index));
                }
            }
        }
        return new(list, multiBranched);
    }

    internal static ExceptionInfo GetInfo(this Exception exception, ref int index, int parentIndex, Func<string, bool>? stackTraceLineFilter)
    {
        stackTraceLineFilter ??= DefaultStackTraceLineFilter;

        var lines = exception.StackTrace is not { } trace
            ? []
            : trace
                .SplitToLines(StringSplitOptions.RemoveEmptyEntries) // NOTE: Do not use StringSplitOptions.TrimEntries here, because we need to trim "   at " prefix later.
                .Select(m => m.TrimStart("   at ").TrimStart()) // see https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Diagnostics/StackTrace.cs,226
                .WhereNot(stackTraceLineFilter)
                .Reverse() // reverse frames to be more readable (from the outermost to the innermost).
                .ToArray();

        var info = new ExceptionInfo(exception.GetType(), exception.Message, lines, index++, parentIndex);
        return info;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="indent">Set the size of an indent (in a number of SPACE characters)</param>
    /// <returns></returns>
    public static string ToFormattedString(this Exception exception, int indent = 3)
    {
        if (exception.IsJustMessage())
            return exception.ToString();

        var (infos, _) = exception.BuildTree();

        return StringBuilder.Build(m =>
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
                    if (indent > 0)
                    {
                        m.Append(' ', indent);
                    }
                    m.AppendLine(line);
                }
            }
        });

    }
}
