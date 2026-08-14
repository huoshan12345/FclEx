namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class UriAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not string stringValue)
            return false;

        if (stringValue.IsNullOrWhiteSpace())
            return AllowEmptyStrings;

        // Require an explicit scheme (e.g. "http://") to avoid platform-specific parsing
        // where Unix may treat "/path" as an absolute file:// URI.
        if (stringValue.Contains(Uri.SchemeDelimiter) == false)
            return false;

        if (Uri.TryCreate(stringValue, UriKind.Absolute, out var uri) == false)
            return false;

        return AllowedSchemes.IsEmpty() || AllowedSchemes.Any(m => string.Equals(m, uri.Scheme, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets or sets the URI schemes accepted by this attribute. An empty array allows any explicit scheme.
    /// </summary>
    public string[] AllowedSchemes { get; set; } = [];

    /// <summary>
    /// Gets or sets a value that indicates whether an empty string is allowed.
    /// </summary>
    public bool AllowEmptyStrings { get; set; } = true;

    public override string FormatErrorMessage(string name)
    {
        var format = AllowedSchemes.IsEmpty()
            ? "The {0} field is not a valid URI."
            : $$"""The {0} field is not a valid URI with one of the allowed schemes: {{AllowedSchemes.JoinWith(", ")}}.""";

        return string.Format(format, name);
    }
}
