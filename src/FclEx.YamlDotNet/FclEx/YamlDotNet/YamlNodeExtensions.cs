namespace FclEx.YamlDotNet;

/// <summary>
/// Provides helpers for working with YAML nodes.
/// </summary>
public static class YamlNodeExtensions
{
    /// <summary>
    /// Determines whether the node is a scalar node with the specified value.
    /// </summary>
    /// <param name="node">The YAML node to inspect.</param>
    /// <param name="value">The scalar value to compare with <see cref="YamlScalarNode.Value"/>, including <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when <paramref name="node"/> is a scalar node with the specified value; otherwise, <see langword="false"/>.</returns>
    public static bool IsScalarWithValue(this YamlNode node, string? value)
    {
        return node is YamlScalarNode scalar && scalar.Value == value;
    }
}
