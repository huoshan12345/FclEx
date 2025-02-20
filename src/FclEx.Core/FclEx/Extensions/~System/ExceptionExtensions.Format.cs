namespace FclEx.Extensions;

/// <summary>
/// Represents detailed information about an exception. br/>
/// NOTE: StackTraceLines will be from the outermost stack trace frame to the innermost, which is opposite from Exception.StackTrace.
/// </summary>
/// <param name="Type">The type of the exception.</param>
/// <param name="Message">The message associated with the exception.</param>
/// <param name="StackTraceLines">The stack trace lines, starting from the outermost frame.</param>
/// <param name="Index">The index of this exception in the context of a collection (e.g., ExceptionTree)</param>
/// <param name="ParentIndex">The index of the parent exception in the context of a collection (if applicable).</param>
public readonly record struct ExceptionInfo(Type Type, string Message, IReadOnlyList<string> StackTraceLines, int Index, int ParentIndex);

/// <summary>
/// Represents a collection of exception information, potentially forming a tree structure.
/// </summary>
/// <param name="Nodes">A list of exception details (nodes) in the tree.</param>
/// <param name="MultiBranched">Indicates if the exceptions form a multi-branched structure.</param>
public readonly record struct ExceptionTree(IReadOnlyList<ExceptionInfo> Nodes, bool MultiBranched);

partial class ExceptionExtensions
{
    /// <summary>
    /// Builds an exception tree from the given exception, including information about the exception 
    /// and any inner exceptions in a hierarchical structure. The method recursively traverses the 
    /// exception chain to capture all relevant details and determine if the tree is multi-branched.
    /// </summary>
    /// <param name="exception">The root exception to start building the tree from.</param>
    /// <param name="stackTraceLineFilter"></param>
    /// <returns>
    /// An <see cref="ExceptionTree"/> containing a list of <see cref="ExceptionInfo"/> nodes 
    /// with details about the exception and its inner exceptions, along with a flag indicating 
    /// whether the tree is multi-branched.
    /// </returns>
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
                        queue.Enqueue((inners[0], index));
                        continue;
                    }
                }

                var info = GetInfo(ex, ref index, parent, stackTraceLineFilter);
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
                var info = GetInfo(ex, ref index, parent, stackTraceLineFilter);
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
                .SplitToLines()
                .Select(m => m.TrimStart("   at ").TrimStart()) // see https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Diagnostics/StackTrace.cs,226
                .Not(stackTraceLineFilter)
                .Reverse() // reverse frames to be more readable (from the outermost to the innermost).
                .ToArray();

        var info = new ExceptionInfo(exception.GetType(), exception.Message, lines, index++, parentIndex);
        return info;
    }

    public static Func<string, bool> DefaultStackTraceLineFilter { get; } = m => ExcludeStackTracePrefixes.Any(m.StartsWith);

    public static IReadOnlyCollection<string> ExcludeStackTracePrefixes { get; } =
    [
        "--- End of ", // --- End of inner exception stack trace --- or --- End of stack trace from previous location ---
        "System.Threading.ExecutionContext.",
        "System.Runtime.CompilerServices.AsyncTaskMethodBuilder",
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
    ];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="indent">Set the size of an indent (in a number of SPACE characters)</param>
    /// <returns></returns>
    public static string ToFormattedString(this Exception exception, int indent = 3)
    {
        var (infos, _) = exception.BuildTree();

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