namespace FclEx.YamlDotNet;

public record YamlSerializeOptions : YamlOptions
{
    public static readonly YamlSerializeOptions Default = new();

    public bool IndentedSequences { get; init; } = true;
}
