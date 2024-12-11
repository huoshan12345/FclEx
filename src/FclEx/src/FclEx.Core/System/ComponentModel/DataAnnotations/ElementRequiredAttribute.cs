namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ElementRequiredAttribute : ValidationAttribute
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public ElementRequiredAttribute() : base(() => "The every element of {0} cannot be null or empty string.")
    {

    }

    /// <summary>
    /// Gets or sets a value that indicates whether an empty string is allowed.
    /// </summary>
    public bool AllowEmptyStrings { get; set; }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var error = new ValidationResult(ErrorMessage);
        var success = ValidationResult.Success!;

        if (value == null)
            return success;

        if (value is not ICollection col)
            return success;

        foreach (var item in col)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (item is null)
                return error;

            // ReSharper disable once InvertIf
            if (item is string str)
            {
                if (AllowEmptyStrings == false && string.IsNullOrWhiteSpace(str))
                    return error;
            }
        }
        return success;
    }
}