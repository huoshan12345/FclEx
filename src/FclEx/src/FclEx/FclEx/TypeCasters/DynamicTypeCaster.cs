using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FclEx.TypeCasters;

public class DynamicTypeCaster : AbstractTypeCaster<DynamicTypeCaster>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("obj"), MaybeNull]
    public sealed override TOutput CastTo<TInput, TOutput>([AllowNull] TInput obj)
    {
        return obj == null
            ? default
            : (TOutput)((dynamic)obj);
    }
}