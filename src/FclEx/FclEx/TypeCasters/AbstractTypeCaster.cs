using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FclEx.TypeCasters
{
    public abstract class AbstractTypeCaster<TCaster> : ITypeCaster
        where TCaster : AbstractTypeCaster<TCaster>, new()
    {
        public static TCaster Instance { get; } = new TCaster();

        [return: MaybeNull]
        public abstract TOutput CastTo<TInput, TOutput>([AllowNull] TInput obj);
    }
}
