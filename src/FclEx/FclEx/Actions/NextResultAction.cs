using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct NextResultAction<T, TNext> : IAction<TNext>
    {
        private readonly IAction<T> _action;
        private readonly Func<OperateResult<T>, IAction<TNext>?> _next;

        public NextResultAction(IAction<T> action, Func<OperateResult<T>, IAction<TNext>?> next)
        {
            _action = Guard.Argument(action, nameof(action)).NotNull().Value;
            _next = Guard.Argument(next, nameof(next)).NotNull();
        }

        public async Task<OperateResult<TNext>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();

            var nextActor = _next(result);
            if (nextActor == null)
                return Constant.NullNextError;

            var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
            return nextResult.WithElapsed(result.Elapsed + nextResult.Elapsed);
        }
    }

    public readonly struct NextResultAction<T> : IAction<T>
    {
        private readonly IAction<T> _action;
        private readonly Func<OperateResult<T>, IAction<T>?> _next;
        private readonly bool _errorWhenNextNull;

        public NextResultAction(IAction<T> action, Func<OperateResult<T>, IAction<T>?> next, bool errorWhenNextNull = true)
        {
            _errorWhenNextNull = errorWhenNextNull;
            _action = Guard.Argument(action, nameof(action)).NotNull().Value;
            _next = Guard.Argument(next, nameof(next)).NotNull();
        }

        public async Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();

            var nextActor = _next(result);
            if (nextActor == null)
            {
                return _errorWhenNextNull
                    ? Constant.NullNextError
                    : result;
            }

            var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
            return nextResult.WithElapsed(result.Elapsed + nextResult.Elapsed);
        }
    }
}
