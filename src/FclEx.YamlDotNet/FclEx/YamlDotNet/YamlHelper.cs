namespace FclEx.YamlDotNet;

public static class YamlHelper
{
    private static readonly ConcurrentDictionary<YamlSerializeOptions, ISerializer> _serializers = new();
    private static readonly ConcurrentDictionary<YamlDeserializeOptions, IDeserializer> _deserializers = new();

    public static ISerializer GetSerializer(YamlSerializeOptions? options = null)
    {
        options ??= YamlSerializeOptions.Default;
        return _serializers.GetOrAdd(options, m =>
        {
            var convention = m.NamingConventionType.ToNamingConvention();
            var builder = new SerializerBuilder()
                .WithNamingConvention(convention);

            if (options.WithTypeConverterAttribute)
                builder.WithTypeConverterAttribute();

            if (options.WithIndentedSequences)
                builder.WithIndentedSequences();

            return builder.Build();
        });
    }

    public static IDeserializer GetDeserializer(YamlDeserializeOptions? options = null)
    {
        options ??= YamlDeserializeOptions.Default;
        return _deserializers.GetOrAdd(options, m =>
        {
            var convention = m.NamingConventionType.ToNamingConvention();
            var builder = new DeserializerBuilder()
                .WithNamingConvention(convention);

            if (options.IgnoreUnmatchedProperties)
                builder.IgnoreUnmatchedProperties();

            if (options.WithTypeConverterAttribute)
                builder.WithTypeConverterAttribute();

            return builder.Build();
        });
    }
}