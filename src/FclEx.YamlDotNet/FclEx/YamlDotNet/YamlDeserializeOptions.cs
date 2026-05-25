namespace FclEx.YamlDotNet;

public record YamlDeserializeOptions : YamlOptions
{
    public static readonly YamlDeserializeOptions Default = new();

    public bool IgnoreUnmatchedProperties { get; init; } = true;
}
