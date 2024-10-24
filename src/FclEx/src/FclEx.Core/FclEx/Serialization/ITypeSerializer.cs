namespace FclEx.Serialization;

public interface ITypeSerializer<TTarget>
{
    TTarget Serialize(object? obj);
    object? Deserialize(TTarget data, Type type);
}