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
            return new SerializerBuilder()
                .WithNamingConvention(convention)
                .Build();
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

            return builder.Build();
        });
    }
}