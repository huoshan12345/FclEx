using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions;

public readonly struct SuccessAction<T> : IAction<T>
{
    private readonly T _obj;
    private readonly TimeSpan _timeSpan;

    public SuccessAction(T obj, TimeSpan timeSpan = default)
    {
        _obj = obj;
        _timeSpan = timeSpan;
    }

    public Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return Operate.CreateSuccess(_obj, _timeSpan);
    }
}