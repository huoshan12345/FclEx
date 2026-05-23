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

        return AllowedSchemas.IsEmpty() || AllowedSchemas.Any(m => string.Equals(m, uri.Scheme, StringComparison.OrdinalIgnoreCase));
    }

    public string[] AllowedSchemas { get; set; } = [];

    /// <summary>
    /// Gets or sets a value that indicates whether an empty string is allowed.
    /// </summary>
    public bool AllowEmptyStrings { get; set; } = true;

    public override string FormatErrorMessage(string name)
    {
        var format = AllowedSchemas.IsEmpty()
            ? "The {0} field is not a valid uri."
            : $$"""The {0} field is not a valid uri with any allowed schemas: {{AllowedSchemas.JoinWith(", ")}}.""";

        return string.Format(format, name);
    }
}