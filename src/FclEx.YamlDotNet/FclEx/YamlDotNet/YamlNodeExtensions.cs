namespace FclEx.YamlDotNet;

public static class YamlNodeExtensions
{
    public static bool IsScalarValue(this YamlNode node, string value)
    {
        return node is YamlScalarNode scalar && scalar.Value == value;
    }
}
