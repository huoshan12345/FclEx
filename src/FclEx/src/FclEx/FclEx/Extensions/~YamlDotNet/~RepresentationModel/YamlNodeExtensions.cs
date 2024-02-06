using YamlDotNet.RepresentationModel;

namespace FclEx.Extensions;

public static class YamlNodeExtensions
{
    public static bool IsScalar(this YamlNode node, string value)
    {
        return node is YamlScalarNode scalar && scalar.Value == value;
    }
}