namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static OperationTransputs<T, TResult> ToOperationTransputs<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> selector)
    {
        var (success, failure) = enumerable
            .Select(m => Transput.Create(m, Operation.Execute(() => selector(m))))
            .Partition(m => m.Output.Success);

        var successItems = success.Select(m => Transput.Create(m.Input, m.Output.Value!)).ToList();
        var failureItems = failure.ToList();
        return new(successItems, failureItems);
    }

    public static async Task<OperationTransputs<T, TResult>> ToOperationTransputs<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> selector)
    {
        var results = await enumerable
            .Select(m => Operation.ExecuteAsync(() => selector(m)).ToTransput(m))
            .WhenAll();

        var (success, failure) = results.Partition(m => m.Output.Success);
        var successItems = success.Select(m => Transput.Create(m.Input, m.Output.Value!)).ToList();
        var failureItems = failure.ToList();
        return new(successItems, failureItems);
    }

    public static Task<OperationTransputs<T, TResult>> ToOperationTransputs<T, TResult>(this IEnumerable<T> enumerable, 
        Func<T, Task<TResult>> taskSelector, int batchSize, CancellationToken token = default)
    {
        return enumerable.ToOperationTransputs(async m => Operation.CreateSuccess(await taskSelector(m)), batchSize, token);
    }

    public static async Task<OperationTransputs<T, TResult>> ToOperationTransputs<T, TResult>(this IEnumerable<T> enumerable, 
        Func<T, Task<OperationResult<TResult>>> taskSelector, int batchSize, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);
        Check.NotLessThan(batchSize, 1);

        var success = new List<Transput<T, TResult>>();
        var failure = new List<Transput<T, OperationResult<TResult>>>();

        foreach (var batch in enumerable.Chunk(batchSize))
        {
            if (token.IsCancellationRequested)
            {
                batch.Select(m => (m, Operation.Cancel<TResult>()))
                    .ForEach(m => failure.Add(m));
            }
            else
            {
                var rs = await batch.Select(async m => (m, await Operation.ExecuteAsync(() => taskSelector(m)))).WhenAll();
                foreach (var (i, o) in rs)
                {
                    if (o.Success)
                        success.Add((i, o.Value!));
                    else
                        failure.Add((i, o));
                }
            }
        }
        return (success, failure);
    }

    public static Task<OperationTransputs<T, TResult>> ToOperationTransputsSerially<T, TResult>(this IEnumerable<T> enumerable,
        Func<T, Task<TResult>> taskSelector, int intervalSeconds = 0, CancellationToken token = default)
    {
        return enumerable.ToOperationTransputsSerially(async m => Operation.CreateSuccess(await taskSelector(m)), intervalSeconds, token);
    }

    public static async Task<OperationTransputs<T, TResult>> ToOperationTransputsSerially<T, TResult>(this IEnumerable<T> enumerable,
        Func<T, Task<OperationResult<TResult>>> taskSelector, int intervalSeconds = 0, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);

        var success = new List<Transput<T, TResult>>();
        var failure = new List<Transput<T, OperationResult<TResult>>>();

        foreach (var item in enumerable)
        {
            if (token.IsCancellationRequested)
            {
                failure.Add((item, Operation.Cancel<TResult>()));
            }
            else
            {
                var r = await Operation.ExecuteAsync(() => taskSelector(item));
                if (r.Success)
                    success.Add((item, r.Value!));
                else
                    failure.Add((item, r));
            }
            await TaskHelper.Delay(intervalSeconds, token);
        }
        return (success, failure);
    }

}