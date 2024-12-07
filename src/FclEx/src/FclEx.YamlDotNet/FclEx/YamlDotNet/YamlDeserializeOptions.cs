namespace FclEx.YamlDotNet;

public class YamlDeserializeOptions : YamlOptions
{
    public static readonly YamlDeserializeOptions Default = new();

    public bool IgnoreUnmatchedProperties { get; set; } = true;
}