namespace FclEx.YamlDotNet;

public static class SerializationBuilderExtensions
{
    internal static IYamlTypeConverter? TryCreateYamlTypeConverter(this Type type)
    {
        if (type.TryGetAttribute<YamlTypeConverterAttribute>(true, out var attribute) == false)
            return null;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (attribute.ConverterType is not { } converterType)
            return null;

        if (converterType.IsAssignableTo(typeof(IYamlTypeConverter)) == false)
            return null;

        var ctor = converterType.GetDefaultConstructor();

        // ReSharper disable once UseNullPropagation
        if (ctor is null)
            return null;

        var converter = ctor.Invoke<IYamlTypeConverter>();
        return converter;
    }

    public static DeserializerBuilder WithTypeConverterAttribute(this DeserializerBuilder builder, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(m => m.IsAbstract == false))
        {
            if (type.TryCreateYamlTypeConverter() is { } converter)
                builder.WithTypeConverter(converter);
        }

        return builder;
    }

    public static DeserializerBuilder WithTypeConverterAttribute(this DeserializerBuilder builder)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            builder.WithTypeConverterAttribute(assembly);
        }
        return builder;
    }

    public static SerializerBuilder WithTypeConverterAttribute(this SerializerBuilder builder, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(m => m.IsAbstract == false))
        {
            if (type.TryCreateYamlTypeConverter() is { } converter)
                builder.WithTypeConverter(converter);
        }

        return builder;
    }

    public static SerializerBuilder WithTypeConverterAttribute(this SerializerBuilder builder)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            builder.WithTypeConverterAttribute(assembly);
        }
        return builder;
    }
}
