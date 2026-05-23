namespace FclEx.YamlDotNet;

public record YamlSerializeOptions : YamlOptions
{
    public static readonly YamlSerializeOptions Default = new();

    public bool WithIndentedSequences { get; set; } = true;
}
