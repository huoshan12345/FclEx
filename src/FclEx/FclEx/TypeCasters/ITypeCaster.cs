using System.Diagnostics.CodeAnalysis;

namespace FclEx.TypeCasters
{
    public interface ITypeCaster
    {
        [return: MaybeNull]
        TOutput CastTo<TInput, TOutput>([AllowNull] TInput obj);
    }
}
