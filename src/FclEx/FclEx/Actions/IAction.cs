using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Actions
{
    public interface IAction<T>
    {
        Task<IOperateResult<T>> ExecuteAsync(CancellationToken token = default);
    }
}
