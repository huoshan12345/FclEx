namespace YamlDotNet.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
public class YamlTypeConverterAttribute(Type converterType) : Attribute
{
    public Type ConverterType { get; } = converterType;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
public class YamlTypeConverterAttribute<T>() : YamlTypeConverterAttribute(typeof(T))
    where T : IYamlTypeConverter;
