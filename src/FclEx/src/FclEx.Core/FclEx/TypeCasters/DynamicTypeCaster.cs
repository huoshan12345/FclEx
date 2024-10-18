namespace FclEx.TypeCasters;

public class DynamicTypeCaster : AbstractTypeCaster<DynamicTypeCaster>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("obj"), MaybeNull]
    public sealed override TOutput CastTo<TInput, TOutput>([AllowNull] TInput obj)
    {
        return obj is null
            ? default
            : (TOutput)((dynamic)obj);
    }
}