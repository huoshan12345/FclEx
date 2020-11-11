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

        public UnionAction(IAction<T> action, Func<T, IAction<TNext>> next)
        {
            _action = Guard.Argument(action, nameof(action)).NotNull().Value;
            _next = Guard.Argument(next, nameof(next)).NotNull();
        }

        public async Task<IOperateResult<(T, TNext)>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();
            if (!result.Successful)
                return result.ToExplicit<(T, TNext)>();

            var nextActor = _next(result.Result);
            if (nextActor == null)
                return OperateResult.CreateSuccess((result.Result, default(TNext)), result.Elapsed);

            var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
            if (!nextResult.Successful)
                return nextResult.ToExplicit<(T, TNext)>();

            return OperateResult.CreateSuccess((result.Result, nextResult.Result), result.Elapsed + nextResult.Elapsed);
        }
    }
}