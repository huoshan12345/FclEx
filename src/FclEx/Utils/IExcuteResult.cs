using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    [Obsolete("使用" + nameof(IOperateResult))]
    public interface IExcuteResult
    {
        bool Successful { get; }
        int Code { get; }
        [JsonIgnore]
        Exception Exception { get; }
        TimeSpan Elapsed { get; }
        string Msg { get; }
        string StackTrace { get; }
    }

    [Obsolete("使用" + nameof(IOperateResult))]
    public interface IExcuteResult<out T> : IExcuteResult
    {
        T Result { get; }
    }
}
