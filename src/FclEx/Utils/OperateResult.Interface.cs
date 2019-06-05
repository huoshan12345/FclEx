using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FclEx.Utils
{
    public interface IOperateResult<out T>
    {
        bool Successful { get; }
        int Code { get; }
        [JsonIgnore]
        Exception Exception { get; }
        TimeSpan Elapsed { get; }
        string Msg { get; }
        string StackTrace { get; }
        T Result { get; }
    }
}
