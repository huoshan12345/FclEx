namespace FclEx.YamlDotNet;

/// <summary>
/// Provides helpers for registering YAML type converters from attributes.
/// </summary>
public static class SerializationBuilderExtensions
{
    /// <summary>
    /// Attempts to create the converter declared by a type's <see cref="YamlTypeConverterAttribute"/>.
    /// </summary>
    /// <param name="type">The attributed type.</param>
    /// <returns>The created converter, or <see langword="null"/> when no usable converter attribute is present.</returns>
    /// <remarks>This method intentionally swallows invalid converter metadata and is used for non-strict probing.</remarks>
    internal static IYamlTypeConverter? TryCreateYamlTypeConverter(this Type type)
    {
        if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out var attribute) == false)
            return null;

        try
        {
            return type.CreateYamlTypeConverter(attribute);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates the converter declared by a type's <see cref="YamlTypeConverterAttribute"/>.
    /// </summary>
    /// <param name="type">The attributed type.</param>
    /// <returns>The created YAML type converter.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the type has no converter attribute, the converter type does not implement <see cref="IYamlTypeConverter"/>,
    /// or the converter type does not have a parameterless constructor.
    /// </exception>
    internal static IYamlTypeConverter CreateYamlTypeConverter(this Type type)
    {
        if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out var attribute) == false)
            throw new InvalidOperationException($"Type '{type.FullName}' does not define a YAML type converter attribute.");

        return type.CreateYamlTypeConverter(attribute);
    }

    /// <summary>
    /// Creates a YAML type converter from an already resolved converter attribute.
    /// </summary>
    private static IYamlTypeConverter CreateYamlTypeConverter(this Type type, YamlTypeConverterAttribute attribute)
    {
        var converterType = attribute.ConverterType;

        if (converterType.IsAssignableTo(typeof(IYamlTypeConverter)) == false)
            throw new InvalidOperationException($"YAML converter type '{converterType.FullName}' for '{type.FullName}' must implement {nameof(IYamlTypeConverter)}.");

        var ctor = converterType.GetParameterlessConstructor();

        if (ctor is null)
            throw new InvalidOperationException($"YAML converter type '{converterType.FullName}' for '{type.FullName}' must have a parameterless constructor.");

        var converter = ctor.Invoke<IYamlTypeConverter>();
        return converter;
    }

    /// <summary>
    /// Registers all non-abstract attributed converters found in an assembly on a deserializer builder.
    /// </summary>
    /// <param name="builder">The deserializer builder to configure.</param>
    /// <param name="assembly">The assembly to scan for <see cref="YamlTypeConverterAttribute"/>.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an attributed type declares an invalid converter.</exception>
    public static DeserializerBuilder WithAttributedTypeConverters(this DeserializerBuilder builder, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(m => m.IsAbstract == false))
        {
            if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out _))
                builder.WithTypeConverter(type.CreateYamlTypeConverter());
        }

        return builder;
    }

    /// <summary>
    /// Registers all non-abstract attributed converters found in the specified assemblies on a deserializer builder.
    /// </summary>
    /// <param name="builder">The deserializer builder to configure.</param>
    /// <param name="assemblies">The assemblies to scan for <see cref="YamlTypeConverterAttribute"/>.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an attributed type declares an invalid converter.</exception>
    public static DeserializerBuilder WithAttributedTypeConverters(this DeserializerBuilder builder, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            builder.WithAttributedTypeConverters(assembly);
        }
        return builder;
    }

    /// <summary>
    /// Registers all non-abstract attributed converters found in currently loaded app-domain assemblies on a deserializer builder.
    /// </summary>
    /// <param name="builder">The deserializer builder to configure.</param>
    /// <returns>The same builder instance.</returns>
    /// <remarks>Prefer the assembly overload when deterministic scanning is important.</remarks>
    /// <exception cref="InvalidOperationException">Thrown when an attributed type declares an invalid converter.</exception>
    public static DeserializerBuilder WithAttributedTypeConvertersFromCurrentAppDomain(this DeserializerBuilder builder)
    {
        return builder.WithAttributedTypeConverters(AppDomain.CurrentDomain.GetAssemblies());
    }

    /// <summary>
    /// Registers all non-abstract attributed converters found in an assembly on a serializer builder.
    /// </summary>
    /// <param name="builder">The serializer builder to configure.</param>
    /// <param name="assembly">The assembly to scan for <see cref="YamlTypeConverterAttribute"/>.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an attributed type declares an invalid converter.</exception>
    public static SerializerBuilder WithAttributedTypeConverters(this SerializerBuilder builder, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(m => m.IsAbstract == false))
        {
            if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out _))
                builder.WithTypeConverter(type.CreateYamlTypeConverter());
        }

        return builder;
    }

    /// <summary>
    /// Registers all non-abstract attributed converters found in the specified assemblies on a serializer builder.
    /// </summary>
    /// <param name="builder">The serializer builder to configure.</param>
    /// <param name="assemblies">The assemblies to scan for <see cref="YamlTypeConverterAttribute"/>.</param>
    /// <returns>The same builder instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an attributed type declares an invalid converter.</exception>
    public static SerializerBuilder WithAttributedTypeConverters(this SerializerBuilder builder, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            builder.WithAttributedTypeConverters(assembly);
        }
        return builder;
    }

    /// <summary>
    /// Registers all non-abstract attributed converters found in currently loaded app-domain assemblies on a serializer builder.
    /// </summary>
    /// <param name="builder">The serializer builder to configure.</param>
    /// <returns>The same builder instance.</returns>
    /// <remarks>Prefer the assembly overload when deterministic scanning is important.</remarks>
    /// <exception cref="InvalidOperationException">Thrown when an attributed type declares an invalid converter.</exception>
    public static SerializerBuilder WithAttributedTypeConvertersFromCurrentAppDomain(this SerializerBuilder builder)
    {
        return builder.WithAttributedTypeConverters(AppDomain.CurrentDomain.GetAssemblies());
    }
}
