using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FclEx.TypeCasters
{
    public class DynamicTypeCaster : AbstractTypeCaster<DynamicTypeCaster>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TOutput CastTo<TInput, TOutput>(TInput obj)
        {
            return (TOutput)((dynamic)obj);
        }
    }
}
