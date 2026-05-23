namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// A validation attribute that checks if a string expression is a valid regular expression.
/// </summary>
public sealed class ValidRegularExpressionAttribute : ValidationAttribute
{
    /// <summary>
    /// Determines whether the specified value of the object is valid.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>An instance of the <see cref="ValidationResult"/> class.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string { Length: > 0 } str) 
            return ValidationResult.Success;

        try
        {
            _ = new Regex(str);
            return ValidationResult.Success;
        }
        catch (ArgumentException)
        {
            return new ValidationResult($"Expression {value} is not a valid regular expression.");
        }
    }
}