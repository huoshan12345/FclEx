using YamlDotNet.Serialization.NamingConventions;

namespace FclEx.YamlDotNet;

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

public static class YamlNamingConventionExtensions
{
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
