namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ElementRequiredAttribute : ValidationAttribute
{
    public int MinLength { get; set; }
    public bool AllowNullElement { get; set; }

    private const string MinLengthError = "The field {0} must be a string or array type with a minimum length of '{1}'.";
    private const string InvalidValueType = "The field {0} of type {1} must be a IEnumerable type.";
    private const string NullElementError = "The field {0} has a null element at {1}.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var success = ValidationResult.Success;

        // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
        if (value == null)
            return success;

        var name = validationContext.DisplayName;

        if (value is string str && str.Length < MinLength)
            return new ValidationResult(string.Format(MinLengthError, name, MinLength));

        if (value is not IEnumerable enumerable)
            return new ValidationResult(string.Format(InvalidValueType, name, value.GetType()));

        var count = 0;
        foreach (var item in enumerable)
        {
            count++;

            if (AllowNullElement == false && item is null)
                return new ValidationResult(string.Format(NullElementError, name, count - 1));

            if (AllowNullElement && count >= MinLength)
                break;
        }

        return count >= MinLength
            ? success
            : new ValidationResult(string.Format(MinLengthError, name, MinLength));
    }
}