using System;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Utils
{
    public interface IOperateResult
    {
        public Exception? Exception { get; }
        public TimeSpan Elapsed { get; }

        [MemberNotNullWhen(false, nameof(Exception))]
        public bool Success => Exception is null;

        [MemberNotNullWhen(true, nameof(Exception))]
        public bool Error => Exception is not null;
    }
}
