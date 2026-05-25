namespace YamlDotNet.Serialization;

/// <summary>
/// Marks a type as using a specific YAML type converter when attributed converter scanning is enabled.
/// </summary>
/// <param name="converterType">The converter type. It must implement <see cref="IYamlTypeConverter"/> and have a parameterless constructor.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
public class YamlTypeConverterAttribute(Type converterType) : Attribute
{
    /// <summary>
    /// Gets the converter type declared by the attribute.
    /// </summary>
    public Type ConverterType { get; } = converterType;
}

/// <summary>
/// Marks a type as using a specific YAML type converter when attributed converter scanning is enabled.
/// </summary>
/// <typeparam name="T">The converter type. It must have a parameterless constructor to be instantiated during scanning.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
public class YamlTypeConverterAttribute<T>() : YamlTypeConverterAttribute(typeof(T))
    where T : IYamlTypeConverter;
