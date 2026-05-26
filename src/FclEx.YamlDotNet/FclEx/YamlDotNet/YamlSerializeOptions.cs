namespace FclEx.YamlDotNet;

/// <summary>
/// Provides options used when creating a YAML serializer.
/// </summary>
public record YamlSerializeOptions : YamlOptions
{
    /// <summary>
    /// Gets the default serializer options.
    /// </summary>
    public static readonly YamlSerializeOptions Default = new();

    /// <summary>
    /// Gets whether sequence items should be indented under their parent key.
    /// </summary>
    public bool IndentedSequences { get; init; } = true;
}
