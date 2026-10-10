namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static OperationIOPairs<T, TResult> ToOperationIOPairs<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> selector)
    {
        var (success, failure) = enumerable
            .Select(m => IOPair.Create(m, Operation.Execute(() => selector(m))))
            .Partition(m => m.Output.IsSuccess);

        var successItems = success.Select(m => IOPair.Create(m.Input, m.Output.Value!)).ToList();
        var failureItems = failure.ToList();
        return new(successItems, failureItems);
    }

    public static async Task<OperationIOPairs<T, TResult>> ToOperationIOPairs<T, TResult>(
        this IEnumerable<T> enumerable,
        Func<T, Task<TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        var results = await enumerable
            .Select(m => Operation.ExecuteAsync(t => selector(m), cancellationToken: cancellationToken).ToIOPair(m))
            .WhenAll();

        var (success, failure) = results.Partition(m => m.Output.IsSuccess);
        var successItems = success.Select(m => IOPair.Create(m.Input, m.Output.Value!)).ToList();
        var failureItems = failure.ToList();
        return new(successItems, failureItems);
    }

    /// <summary>
    /// Executes the asynchronous operations in batches and separates their successful outputs from failed inputs.
    /// </summary>
    /// <remarks>
    /// If <paramref name="cancellationToken"/> is canceled, the remaining source items are still enumerated and each
    /// receives a canceled result. This preserves one result per input item, but requires a finite source because cancellation
    /// does not stop enumeration. The selector is not invoked for those remaining items.
    /// </remarks>
    public static async Task<OperationIOPairs<T, TResult>> ToOperationIOPairs<T, TResult>(
        this IEnumerable<T> enumerable,
        Func<T, Task<OperationResult<TResult>>> taskSelector,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);
        Check.NotLessThan(batchSize, 1);

        var success = new List<IOPair<T, TResult>>();
        var failure = new List<IOPair<T, OperationResult<TResult>>>();

        foreach (var batch in enumerable.Chunk(batchSize))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                batch.Select(m => (m, Operation.Cancel<TResult>()))
                    .ForEach(m => failure.Add(m));
            }
            else
            {
                var rs = await batch.Select(m => Operation.ExecuteAsync(t => taskSelector(m), cancellationToken: cancellationToken)
                        .Then(x => (m, x)))
                    .WhenAll();

                foreach (var (i, o) in rs)
                {
                    if (o.IsSuccess)
                        success.Add((i, o.Value!));
                    else
                        failure.Add((i, o));
                }
            }
        }
        return (success, failure);
    }

    public static Task<OperationIOPairs<T, TResult>> ToOperationIOPairs<T, TResult>(
        this IEnumerable<T> enumerable,
        Func<T, Task<TResult>> taskSelector,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return enumerable.ToOperationIOPairs(m => 
            Operation.ExecuteAsync(t => taskSelector(m), cancellationToken: cancellationToken), batchSize, cancellationToken);
    }

    public static Task<OperationIOPairs<T, TResult>> ToOperationIOPairsSerially<T, TResult>(
        this IEnumerable<T> enumerable,
        Func<T, Task<TResult>> taskSelector,
        TimeSpan interval = default,
        CancellationToken cancellationToken = default)
    {
        return enumerable.ToOperationIOPairsSerially(m => 
            Operation.ExecuteAsync(t => taskSelector(m), cancellationToken: cancellationToken), interval, cancellationToken);
    }

    /// <summary>
    /// Executes the asynchronous operations one at a time and separates their successful outputs from failed inputs.
    /// </summary>
    /// <remarks>
    /// If <paramref name="cancellationToken"/> is canceled, the remaining source items are still enumerated and each receives a canceled
    /// result. This preserves one result per input item, but requires a finite source because cancellation does not stop
    /// enumeration. The selector is not invoked for those remaining items. A positive <paramref name="interval"/> is applied
    /// only between consecutive operations, never after the final item.
    /// </remarks>
    public static async Task<OperationIOPairs<T, TResult>> ToOperationIOPairsSerially<T, TResult>(
        this IEnumerable<T> enumerable,
        Func<T, Task<OperationResult<TResult>>> taskSelector,
        TimeSpan interval = default,
        CancellationToken cancellationToken = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);

        var success = new List<IOPair<T, TResult>>();
        var failure = new List<IOPair<T, OperationResult<TResult>>>();

        using var enumerator = enumerable.GetEnumerator();
        if (enumerator.MoveNext() == false)
            return (success, failure);

        while (true)
        {
            var item = enumerator.Current;
            if (cancellationToken.IsCancellationRequested)
            {
                failure.Add((item, Operation.Cancel<TResult>()));
            }
            else
            {
                var r = await Operation.ExecuteAsync(t => taskSelector(item), cancellationToken: cancellationToken);
                if (r.IsSuccess)
                    success.Add((item, r.Value!));
                else
                    failure.Add((item, r));
            }

            if (enumerator.MoveNext() == false)
                break;

            await Task.DelaySafely(interval, cancellationToken);
        }
        return (success, failure);
    }

}
