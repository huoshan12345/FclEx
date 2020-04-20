using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public interface IOperateResult
    {
        bool Successful { get; }
        int Code { get; }
        Exception? Exception { get; }
        TimeSpan Elapsed { get; }
        OperateResult<TTarget> ToExplicit<TTarget>();
        IOperateResult WithElapsed(TimeSpan span);
    }

    public interface IOperateResult<out T> : IOperateResult
    {
        T Result { get; }
    }
}
