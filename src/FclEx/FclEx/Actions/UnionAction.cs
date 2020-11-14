using System;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct UnionAction<T, TNext> : IAction<(T, TNext)>
    {
        private readonly IAction<T> _action;
        private readonly Func<T, IAction<TNext>> _next;
        private readonly bool _errorWhenNextNull;
        private readonly bool _prevWhenNextError;

        public UnionAction(IAction<T> action, Func<T, IAction<TNext>> next,
            bool errorWhenNextNull = true, bool prevWhenNextError = false)
        {
            _action = Guard.Argument(action, nameof(action)).NotNull().Value;
            _next = Guard.Argument(next, nameof(next)).NotNull();
            _errorWhenNextNull = errorWhenNextNull;
            _prevWhenNextError = prevWhenNextError;
        }

        public async Task<OperateResult<(T, TNext)>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();
            if (!result.Successful)
                return result.ToExplicit<(T, TNext)>();

            var item = result.Result!;
            var nextActor = _next(item);
            if (nextActor == null)
            {
                return _errorWhenNextNull
                    ? (OperateResult<(T, TNext)>)Constant.NullNextError
#pragma warning disable 8619
                    : ((item, default), result.Elapsed);
#pragma warning restore 8619
            }

            var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
            if (!nextResult.Successful)
                return _prevWhenNextError
#pragma warning disable 8619
                    ? ((item, default), result.Elapsed)
#pragma warning restore 8619
                    : nextResult.ToExplicit<(T, TNext)>();

            return ((item, nextResult.Result!), result.Elapsed + nextResult.Elapsed);
        }
    }
}