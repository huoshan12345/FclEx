using System.Collections.Generic;

namespace FclEx.TypeCasters;

public abstract class AbstractTypeCaster<TCaster> : ITypeCaster
    where TCaster : AbstractTypeCaster<TCaster>, new()
{
    public static TCaster Instance { get; } = new();

    public abstract TOutput? CastTo<TInput, TOutput>(TInput? obj);
}