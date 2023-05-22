using System;
using System.Diagnostics;
using FclEx.Helpers;

namespace FclEx.Utils;

public static partial class Operate
{
    public static OperateResult Execute(Action action)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            action();
            return CreateSuccess(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static OperateResult<T> Execute<T>(Func<T> action)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = action();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static OperateResult Execute(Func<OperateResult> action) => Execute<OperateResult>(action).Unwrap();

    public static OperateResult<T> Execute<T>(Func<OperateResult<T>> action) => Execute<OperateResult<T>>(action).Unwrap();
}