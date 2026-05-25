namespace FclEx.YamlDotNet;

public static class SerializationBuilderExtensions
{
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

    internal static IYamlTypeConverter CreateYamlTypeConverter(this Type type)
    {
        if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out var attribute) == false)
            throw new InvalidOperationException($"Type '{type.FullName}' does not define a YAML type converter attribute.");

        return type.CreateYamlTypeConverter(attribute);
    }

    private static IYamlTypeConverter CreateYamlTypeConverter(this Type type, YamlTypeConverterAttribute attribute)
    {
        var converterType = attribute.ConverterType;

        if (converterType.IsAssignableTo(typeof(IYamlTypeConverter)) == false)
            throw new InvalidOperationException($"YAML converter type '{converterType.FullName}' for '{type.FullName}' must implement {nameof(IYamlTypeConverter)}.");

        var ctor = converterType.GetDefaultConstructor();

        if (ctor is null)
            throw new InvalidOperationException($"YAML converter type '{converterType.FullName}' for '{type.FullName}' must have a parameterless constructor.");

        var converter = ctor.Invoke<IYamlTypeConverter>();
        return converter;
    }

    public static DeserializerBuilder WithAttributedTypeConverters(this DeserializerBuilder builder, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(m => m.IsAbstract == false))
        {
            if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out _))
                builder.WithTypeConverter(type.CreateYamlTypeConverter());
        }

        return builder;
    }

    public static DeserializerBuilder WithAttributedTypeConverters(this DeserializerBuilder builder, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            builder.WithAttributedTypeConverters(assembly);
        }
        return builder;
    }

    public static DeserializerBuilder WithAttributedTypeConvertersFromCurrentAppDomain(this DeserializerBuilder builder)
    {
        return builder.WithAttributedTypeConverters(AppDomain.CurrentDomain.GetAssemblies());
    }

    public static SerializerBuilder WithAttributedTypeConverters(this SerializerBuilder builder, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(m => m.IsAbstract == false))
        {
            if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out _))
                builder.WithTypeConverter(type.CreateYamlTypeConverter());
        }

        return builder;
    }

    public static SerializerBuilder WithAttributedTypeConverters(this SerializerBuilder builder, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            builder.WithAttributedTypeConverters(assembly);
        }
        return builder;
    }

    public static SerializerBuilder WithAttributedTypeConvertersFromCurrentAppDomain(this SerializerBuilder builder)
    {
        return builder.WithAttributedTypeConverters(AppDomain.CurrentDomain.GetAssemblies());
    }
}
