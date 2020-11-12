using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        void Deconstruct(out bool successful, out TimeSpan elapsed, out Exception? ex);
    }

    public interface IOperateResult<T> : IOperateResult
    {
        [AllowNull, MaybeNull] T Result { get; }
        void Deconstruct(out bool successful, out TimeSpan elapsed, [MaybeNull] out T obj, out Exception? ex);
    }
}
