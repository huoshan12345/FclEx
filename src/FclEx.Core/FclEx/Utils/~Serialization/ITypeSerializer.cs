namespace FclEx.Utils;

public interface ITypeSerializer<TTarget>
{
    TTarget Serialize(object? obj, Type type);
    object? Deserialize(TTarget data, Type type);
}