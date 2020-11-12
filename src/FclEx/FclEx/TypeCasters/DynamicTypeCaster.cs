using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace FclEx.TypeCasters
{
    public class DynamicTypeCaster : AbstractTypeCaster<DynamicTypeCaster>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: MaybeNull]
        public override TOutput CastTo<TInput, TOutput>([AllowNull] TInput obj)
        {
            return obj == null 
                ? default 
                : (TOutput)((dynamic)obj);
        }
    }
}
