using System.Diagnostics.CodeAnalysis;

namespace FclEx.TypeCasters;

public interface ITypeCaster
{
    TOutput? CastTo<TInput, TOutput>(TInput? obj);
}