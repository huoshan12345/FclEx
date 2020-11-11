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
        OperateResult<TDest> ToExplicit<TDest>();
        IOperateResult WithElapsed(TimeSpan span);
        void Deconstruct(out bool successful, out TimeSpan elapsed, out Exception? ex);
    }

    public interface IOperateResult<T> : IOperateResult
    {
        T Result { get; }
        new IOperateResult<T> WithElapsed(TimeSpan span);
        void Deconstruct(out bool successful, out TimeSpan elapsed, out T obj, out Exception? ex);
    }
}
