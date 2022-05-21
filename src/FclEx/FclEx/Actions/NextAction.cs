using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct NextAction<T, TNext> : IAction<TNext>
    {
        private readonly IAction<T> _action;
        private readonly Func<T, IAction<TNext>?> _next;

        public NextAction(IAction<T> action, Func<T, IAction<TNext>?> next)
        {
            _action = Check.NotNull(action);
            _next = Check.NotNull(next);
        }

        public async Task<OperateResult<TNext>> ExecuteAsync(CancellationToken token = default)
        {
            var result = await _action.ExecuteAsync(token).DonotCapture();
            if (!result.Success)
                return result.ToExplicit<TNext>();

            var nextActor = _next(result.Value);
            if (nextActor == null)
                return Constant.NullNextError;

            var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
            if (!nextResult.Success)
                return nextResult;

            return nextResult.Elapsed(result.Elapsed + nextResult.Elapsed);
        }
    }

    //public readonly struct NextAction<T> : IAction<T>
    //{
    //    private readonly IAction<T> _action;
    //    private readonly Func<T, IAction<T>?> _next;
    //    private readonly bool _errorWhenNextNull;
    //    private readonly bool _prevWhenNextError;

    //    public NextAction(IAction<T> action, Func<T, IAction<T>?> next,
    //        bool errorWhenNextNull, bool prevWhenNextError)
    //    {
    //        _errorWhenNextNull = errorWhenNextNull;
    //        _prevWhenNextError = prevWhenNextError;
    //        _action = Check.NotNull(action).Value;
    //        _next = Check.NotNull(next);
    //    }

    //    public async Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
    //    {
    //        var result = await _action.ExecuteAsync(token).DonotCapture();
    //        if (!result.Success)
    //            return result;

    //        var nextActor = _next(result.Value!);
    //        if (nextActor == null)
    //        {
    //            return _errorWhenNextNull
    //                ? Constant.NullNextError
    //                : result;
    //        }

    //        var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
    //        if (!nextResult.Success)
    //            return _prevWhenNextError
    //                ? result
    //                : nextResult;

    //        return nextResult.WithElapsed(result.Elapsed + nextResult.Elapsed);
    //    }
    //}
}
