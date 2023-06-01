using System.Threading.Tasks;
using Polly;

namespace FclEx.Extensions;

public static class AsyncPolicyExtensions
{
    public static Task<OperateResult<T>> OperateExecuteAsync<T>(this IAsyncPolicy<T> policy, Func<Task<T>> action)
    {
        return Operate.ExecuteAsync(() => policy.ExecuteAsync(action));
    }

    public static Task<OperateResult<T>> OperateExecuteAsync<T>(this IAsyncPolicy<OperateResult<T>> policy, Func<Task<OperateResult<T>>> action)
    {
        return Operate.ExecuteAsync(() => policy.ExecuteAsync(action));
    }
}