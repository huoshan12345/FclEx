using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Utils;
using MoreLinq;

namespace FclEx
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    partial class EnumerableExtensions
    {
        public static Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
            ToParallellyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> taskSelector,
         int batchSize, CancellationToken token = default)
        {
            return enumerable.ToParallellyExecutedTaskOfPair(async m => OperateResult.CreateSuccess(await taskSelector(m)), batchSize, token);
        }

        public static async Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
            ToParallellyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<OperateResult<TResult>>> taskSelector,
                int batchSize, CancellationToken token = default)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(taskSelector, nameof(taskSelector)).NotNull();
            Guard.Argument(batchSize, nameof(batchSize)).Min(1);

            var success = new List<(T, TResult)>();
            var failure = new List<(T, OperateResult<TResult>)>();
            foreach (var batch in enumerable.Batch(batchSize))
            {
                if (token.IsCancellationRequested)
                {
                    failure.AddRange(batch.Select(m => (m, OperateResult.CreateCancel<TResult>())));
                }
                else
                {
                    var rs = await batch.Select(async m => (m, await OperateResult.ExcuteAsync(() => taskSelector(m)))).WhenAll();
                    foreach (var (i, o) in rs)
                    {
                        if (o.Successful)
                            success.Add((i, o.Result!));
                        else
                            failure.Add((i, o));
                    }
                }
            }
            return (success, failure);
        }

        public static Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
            ToSeriallyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> taskSelector,
                int intervalSeconds = 0, CancellationToken token = default)
        {
            return enumerable.ToSeriallyExecutedTaskOfPair(async m => OperateResult.CreateSuccess(await taskSelector(m)), intervalSeconds, token);
        }

        public static async Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
            ToSeriallyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<OperateResult<TResult>>> taskSelector,
                int intervalSeconds = 0, CancellationToken token = default)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(taskSelector, nameof(taskSelector)).NotNull();

            var success = new List<(T, TResult)>();
            var failure = new List<(T, OperateResult<TResult>)>();

            foreach (var item in enumerable)
            {
                if (token.IsCancellationRequested)
                {
                    failure.Add((item, OperateResult.CreateCancel<TResult>()));
                }
                else
                {
                    var r = await OperateResult.ExcuteAsync(() => taskSelector(item));
                    if (r.Successful)
                        success.Add((item, r.Result!));
                    else
                        failure.Add((item, r));
                }
                await TaskHelper.Delay(intervalSeconds, token);
            }
            return (success, failure);
        }

        public static async Task ToSeriallyExecutedTask<T>(this IEnumerable<T> enumerable, Func<T, Task> taskSelector,
                int intervalSeconds = 0, CancellationToken token = default)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(taskSelector, nameof(taskSelector)).NotNull();

            foreach (var item in enumerable)
            {
                if (token.IsCancellationRequested)
                    break;

                await taskSelector(item);
                await TaskHelper.Delay(intervalSeconds, token);
            }
        }

        public static async Task<List<TResult>> ToSeriallyExecutedTask<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> taskSelector,
            int intervalSeconds = 0, CancellationToken token = default)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(taskSelector, nameof(taskSelector)).NotNull();

            var list = new List<TResult>();
            foreach (var item in enumerable)
            {
                if (token.IsCancellationRequested)
                    break;

                var r = await taskSelector(item);
                list.Add(r);
                await TaskHelper.Delay(intervalSeconds, token);
            }
            return list;
        }

        public static async Task<OperateResult<List<T>>> ToSeriallyExecutedTask<T>(this IEnumerable<T> enumerable,
            Func<T, Task<OperateResult<T>>> taskSelector, int intervalSeconds = 0, CancellationToken token = default, bool terminateOnFirstError = false)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(taskSelector, nameof(taskSelector)).NotNull();
            var span = TimeSpan.Zero;
            var list = new List<T>();
            IList<Exception>? exceptions = null;
            foreach (var obj in enumerable)
            {
                if (!token.IsCancellationRequested)
                {
                    var r = await taskSelector(obj).DonotCapture();
                    span += r.Elapsed;
                    if (r.Successful)
                    {
                        list.Add(r.Result!);
                    }
                    else
                    {
                        if (terminateOnFirstError)
                        {
                            return r.ToExplicit<List<T>>();
                        }
                        else
                        {
                            exceptions ??= new List<Exception>();
                            exceptions.Add(r.Exception!);
                        }
                    }
                    await TaskHelper.Delay(intervalSeconds, token);

                }
                else
                {
                    break;
                }
            }
            if (exceptions.IsValid())
            {
                return (new AggregateException(exceptions), span);
            }
            else
            {
                return (list, span);
            }
        }

        public static async Task<List<TResult>> ToParallellyExecutedTask<T, TResult>(this IEnumerable<T> enumerable,
            Func<T, Task<TResult>> taskSelector, int batchSize, CancellationToken token = default)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(taskSelector, nameof(taskSelector)).NotNull();
            Guard.Argument(batchSize, nameof(batchSize)).Min(1);

            var list = new List<TResult>();
            foreach (var batch in enumerable.Batch(batchSize))
            {
                if (token.IsCancellationRequested)
                    break;

                var rs = await batch.Select(taskSelector).WhenAll();
                list.AddRange(rs);
            }
            return list;
        }

        public static async Task ToParallellyExecutedTask<T>(this IEnumerable<T> enumerable,
            Func<T, Task> taskSelector, int batchSize, CancellationToken token = default)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(taskSelector, nameof(taskSelector)).NotNull();
            Guard.Argument(batchSize, nameof(batchSize)).Min(1);

            foreach (var batch in enumerable.Batch(batchSize))
            {
                if (token.IsCancellationRequested)
                    break;
                await batch.Select(taskSelector).WhenAll();
            }
        }
    }
}
