using YamlDotNet.Serialization.NamingConventions;

namespace FclEx.YamlDotNet;

public enum NamingConventionType
{
    /// <summary>
    /// Convert the string from camelcase (thisIsATest) to an underscored (this_is_a_test) string
    /// </summary>
    Underscored,
    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// camel case (thisIsATest). Camel case is the same as Pascal case, except the first letter
    /// is lowercase.
    /// </summary>
    CamelCase,
    /// <summary>
    /// Convert the string from camelcase (thisIsATest) to a hyphenated (this-is-a-test) string
    /// </summary>
    Hyphenated,
    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// lower case (thisisatest).
    /// </summary>
    LowerCase,
    /// <summary>
    /// Performs no naming conversion.
    /// </summary>
    Null,
    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// pascal case (ThisIsATest). Pascal case is the same as camel case, except the first letter
    /// is uppercase.
    /// </summary>
    PascalCase,
}

public static class NamingConventionExtensions
{
    public static INamingConvention ToNamingConvention(this NamingConventionType type)
    {
        return type switch
        {
            NamingConventionType.Underscored => UnderscoredNamingConvention.Instance,
            NamingConventionType.CamelCase => CamelCaseNamingConvention.Instance,
            NamingConventionType.Hyphenated => HyphenatedNamingConvention.Instance,
            NamingConventionType.LowerCase => LowerCaseNamingConvention.Instance,
            NamingConventionType.Null => NullNamingConvention.Instance,
            NamingConventionType.PascalCase => PascalCaseNamingConvention.Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}