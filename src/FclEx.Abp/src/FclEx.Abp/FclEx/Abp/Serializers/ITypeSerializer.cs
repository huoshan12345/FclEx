using System;

namespace FclEx.Abp.Serializers;

public interface ITypeSerializer<TTarget>
{
    TTarget Serialize(object? obj);
    object? Deserialize(TTarget data, Type type);
}