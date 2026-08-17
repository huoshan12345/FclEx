namespace FclEx.YamlDotNet;

/// <summary>
/// Provides options used when creating a YAML deserializer.
/// </summary>
public record YamlDeserializeOptions : YamlOptions
{
    /// <summary>
    /// Gets the default deserializer options.
    /// </summary>
    public static readonly YamlDeserializeOptions Default = new();

    /// <summary>
    /// Gets whether YAML keys without matching .NET members should be ignored.
    /// Set to <see langword="false"/> to make unmatched keys fail deserialization.
    /// </summary>
    public bool IgnoreUnmatchedProperties { get; init; } = true;
}
