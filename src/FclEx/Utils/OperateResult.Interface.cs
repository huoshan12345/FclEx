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
        [JsonIgnore]
        Exception Exception { get; }
        TimeSpan Elapsed { get; }
        string Msg { get; }
        string StackTrace { get; }
        OperateResult<TTarget> ToExplicit<TTarget>();
    }

    public interface IOperateResult<out T> : IOperateResult
    {
        T Result { get; }
    }
}
