using YamlDotNet.Serialization.NamingConventions;

namespace FclEx.YamlDotNet;

/// <summary>
/// Identifies a YamlDotNet naming convention by a stable enum value.
/// </summary>
public enum YamlNamingConvention
{
    /// <summary>
    /// Performs no naming conversion.
    /// </summary>
    None,
    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// camel case (thisIsATest). Camel case is the same as Pascal case, except the first letter
    /// is lowercase.
    /// </summary>
    CamelCase,
    /// <summary>
    /// Convert the string from camelcase (thisIsATest) to a hyphenated (this-is-a-test) string
    /// </summary>
    KebabCase,
    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// lower case (thisisatest).
    /// </summary>
    LowerCase,
    /// <summary>
    /// Convert the string from camelcase (thisIsATest) to an underscored (this_is_a_test) string
    /// </summary>
    SnakeCase,
    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// pascal case (ThisIsATest). Pascal case is the same as camel case, except the first letter
    /// is uppercase.
    /// </summary>
    PascalCase,
}

/// <summary>
/// Provides conversion helpers for <see cref="YamlNamingConvention"/>.
/// </summary>
public static class YamlNamingConventionExtensions
{
    /// <summary>
    /// Converts a <see cref="YamlNamingConvention"/> value to the matching YamlDotNet naming convention instance.
    /// </summary>
    /// <param name="convention">The convention value to convert.</param>
    /// <returns>The matching YamlDotNet naming convention.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="convention"/> is not a defined value.</exception>
    public static INamingConvention ToNamingConvention(this YamlNamingConvention convention)
    {
        return convention switch
        {
            YamlNamingConvention.None => NullNamingConvention.Instance,
            YamlNamingConvention.CamelCase => CamelCaseNamingConvention.Instance,
            YamlNamingConvention.KebabCase => HyphenatedNamingConvention.Instance,
            YamlNamingConvention.LowerCase => LowerCaseNamingConvention.Instance,
            YamlNamingConvention.SnakeCase => UnderscoredNamingConvention.Instance,
            YamlNamingConvention.PascalCase => PascalCaseNamingConvention.Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(convention), convention, null)
        };
    }
}
