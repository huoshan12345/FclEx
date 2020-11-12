using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct TryNextAction<T> : IAction<T>
    {
        private readonly IAction<T> _action;
        private readonly Func<T, IAction<T>> _next;

        public TryNextAction(IAction<T> action, Func<T, IAction<T>> next)
        {
            _action = Guard.Argument(action, nameof(action)).NotNull().Value;
            _next = Guard.Argument(next, nameof(next)).NotNull();
        }

        public async Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();
            if (!result.Successful)
                return result;

            var nextActor = _next(result.Result!);
            if (nextActor == null)
                return result;

            var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
            if (!nextResult.Successful)
                return nextResult;

            return nextResult.WithElapsed(result.Elapsed + nextResult.Elapsed);
        }
    }
}
