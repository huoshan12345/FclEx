namespace FclEx.YamlDotNet;

/// <summary>
/// Creates and caches configured YAML serializers and deserializers.
/// </summary>
public static class YamlHelper
{
    private static readonly ConcurrentDictionary<YamlSerializeOptions, ISerializer> _serializers = new();
    private static readonly ConcurrentDictionary<YamlDeserializeOptions, IDeserializer> _deserializers = new();

    /// <summary>
    /// Gets a serializer for the specified options.
    /// </summary>
    /// <param name="options">The serializer options. When <c>null</c>, <see cref="YamlSerializeOptions.Default"/> is used.</param>
    /// <returns>A cached serializer configured with the requested options.</returns>
    /// <remarks>
    /// Serializer instances are cached by the option record value. Attribute-based converter scanning is only applied when
    /// <see cref="YamlOptions.UseTypeConverterAttributes"/> is enabled.
    /// </remarks>
    public static ISerializer GetSerializer(YamlSerializeOptions? options = null)
    {
        options ??= YamlSerializeOptions.Default;
        return _serializers.GetOrAdd(options, m =>
        {
            var convention = m.NamingConvention.ToNamingConvention();
            var builder = new SerializerBuilder()
                .WithNamingConvention(convention);

            if (options.UseTypeConverterAttributes)
                builder.WithAttributedTypeConverters(options.TypeConverterAssemblies ?? AppDomain.CurrentDomain.GetAssemblies());

            if (options.IndentedSequences)
                builder.WithIndentedSequences();

            return builder.Build();
        });
    }

    /// <summary>
    /// Gets a deserializer for the specified options.
    /// </summary>
    /// <param name="options">The deserializer options. When <c>null</c>, <see cref="YamlDeserializeOptions.Default"/> is used.</param>
    /// <returns>A cached deserializer configured with the requested options.</returns>
    /// <remarks>
    /// Deserializer instances are cached by the option record value. Attribute-based converter scanning is only applied when
    /// <see cref="YamlOptions.UseTypeConverterAttributes"/> is enabled.
    /// </remarks>
    public static IDeserializer GetDeserializer(YamlDeserializeOptions? options = null)
    {
        options ??= YamlDeserializeOptions.Default;
        return _deserializers.GetOrAdd(options, m =>
        {
            var convention = m.NamingConvention.ToNamingConvention();
            var builder = new DeserializerBuilder()
                .WithNamingConvention(convention);

            if (options.IgnoreUnmatchedProperties)
                builder.IgnoreUnmatchedProperties();

            if (options.UseTypeConverterAttributes)
                builder.WithAttributedTypeConverters(options.TypeConverterAssemblies ?? AppDomain.CurrentDomain.GetAssemblies());

            return builder.Build();
        });
    }
}
