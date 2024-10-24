namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static OperateTransputs<T, TResult> ToOperateTransputs<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> selector)
    {
        var (success, failure) = enumerable
            .Select(m => Transput.Create(m, Operate.Execute(() => selector(m))))
            .Partition(m => m.Output.Success);

        var successItems = success.Select(m => Transput.Create(m.Input, m.Output.Value!)).ToList();
        var failureItems = failure.ToList();
        return new(successItems, failureItems);
    }

    public static async Task<OperateTransputs<T, TResult>> ToOperateTransputs<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> selector)
    {
        var results = await enumerable
            .Select(m => Operate.ExecuteAsync(() => selector(m)).ToTransput(m))
            .WhenAll();

        var (success, failure) = results.Partition(m => m.Output.Success);
        var successItems = success.Select(m => Transput.Create(m.Input, m.Output.Value!)).ToList();
        var failureItems = failure.ToList();
        return new(successItems, failureItems);
    }

    public static Task<OperateTransputs<T, TResult>> ToOperateTransputs<T, TResult>(this IEnumerable<T> enumerable, 
        Func<T, Task<TResult>> taskSelector, int batchSize, CancellationToken token = default)
    {
        return enumerable.ToOperateTransputs(async m => Operate.CreateSuccess(await taskSelector(m)), batchSize, token);
    }

    public static async Task<OperateTransputs<T, TResult>> ToOperateTransputs<T, TResult>(this IEnumerable<T> enumerable, 
        Func<T, Task<OperateResult<TResult>>> taskSelector, int batchSize, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);
        Check.NotLessThan(batchSize, 1);

        var success = new List<Transput<T, TResult>>();
        var failure = new List<Transput<T, OperateResult<TResult>>>();

        foreach (var batch in enumerable.Chunk(batchSize))
        {
            if (token.IsCancellationRequested)
            {
                batch.Select(m => (m, Operate.CreateCancel<TResult>()))
                    .ForEach(m => failure.Add(m));
            }
            else
            {
                var rs = await batch.Select(async m => (m, await Operate.ExecuteAsync(() => taskSelector(m)))).WhenAll();
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

    public static Task<OperateTransputs<T, TResult>> ToOperateTransputsSerially<T, TResult>(this IEnumerable<T> enumerable,
        Func<T, Task<TResult>> taskSelector, int intervalSeconds = 0, CancellationToken token = default)
    {
        return enumerable.ToOperateTransputsSerially(async m => Operate.CreateSuccess(await taskSelector(m)), intervalSeconds, token);
    }

    public static async Task<OperateTransputs<T, TResult>> ToOperateTransputsSerially<T, TResult>(this IEnumerable<T> enumerable,
        Func<T, Task<OperateResult<TResult>>> taskSelector, int intervalSeconds = 0, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);

        var success = new List<Transput<T, TResult>>();
        var failure = new List<Transput<T, OperateResult<TResult>>>();

        foreach (var item in enumerable)
        {
            if (token.IsCancellationRequested)
            {
                failure.Add((item, Operate.CreateCancel<TResult>()));
            }
            else
            {
                var r = await Operate.ExecuteAsync(() => taskSelector(item));
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