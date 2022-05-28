using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;
using FclEx.Utils;

namespace FclEx.Actions;

public class CommonAction
{
    public static CommonAction<T> Create<T>(Func<CancellationToken, T> func, bool excuteSafely = true)
    {
        return new(t => Operate.CreateSuccess(func(t)), excuteSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, Task<T>> func, bool excuteSafely = true)
    {
        return new(async t => Operate.CreateSuccess(await func(t).DonotCapture()), excuteSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, OperateResult<T>> func, bool excuteSafely = true)
    {
        return new(t => func(t).ToTask(), excuteSafely);
    }

    public static CommonAction<T> Create<T>(Func<CancellationToken, Task<OperateResult<T>>> func, bool excuteSafely = true)
    {
        return new(func, excuteSafely);
    }

    public static VoidCommonAction Create(Action<CancellationToken> func, bool excuteSafely = true)
    {
        return new(t =>
        {
            func(t);
            return Operate.CreateSuccess(default(Unit)).ToTask();
        }, excuteSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, Task> func, bool excuteSafely = true)
    {
        return new(async t =>
        {
            await func(t).DonotCapture();
            return Operate.CreateSuccess(default(Unit));
        }, excuteSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, OperateResult> func, bool excuteSafely = true)
    {
        return new(t => func(t).ToTask(), excuteSafely);
    }

    public static VoidCommonAction Create(Func<CancellationToken, Task<OperateResult>> func, bool excuteSafely = true)
    {
        return new(async t => await func(t).DonotCapture(), excuteSafely);
    }
}

public readonly struct CommonAction<T> : IAction<T>
{
    private readonly bool _excuteSafely;
    private readonly Func<CancellationToken, Task<OperateResult<T>>> _func;

    public CommonAction(Func<CancellationToken, Task<OperateResult<T>>> func, bool excuteSafely)
    {
        _excuteSafely = excuteSafely;
        _func = Check.NotNull(func);
    }

    public Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        var func = _func;
        return _excuteSafely
            ? Operate.ExcuteAsync(() => func(token))
            : func(token);
    }
}

public readonly struct VoidCommonAction : IAction<Unit>
{
    private readonly bool _excuteSafely;
    private readonly Func<CancellationToken, Task<OperateResult>> _func;

    public VoidCommonAction(Func<CancellationToken, Task<OperateResult>> func, bool excuteSafely)
    {
        _excuteSafely = excuteSafely;
        _func = Check.NotNull(func);
    }

    public Task<OperateResult<Unit>> ExecuteAsync(CancellationToken token = default)
    {
        var func = _func;
        return _excuteSafely
            ? Operate.ExcuteAsync(() => func(token))
            : func(token);
    }
}