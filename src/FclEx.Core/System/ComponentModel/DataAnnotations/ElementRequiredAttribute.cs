namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ElementRequiredAttribute : ValidationAttribute
{
    private int _minLength;

    /// <summary>
    /// Gets or sets the minimum number of elements required. Zero is valid and imposes no minimum length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The assigned value is negative.</exception>
    public int MinLength
    {
        get => _minLength;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Minimum length cannot be negative.");

            _minLength = value;
        }
    }

    public bool AllowNullElement { get; set; } = true;

    private const string MinLengthError = "The field {0} must be a string or array type with a minimum length of '{1}'.";
    private const string InvalidValueType = "The field {0} of type {1} must be a IEnumerable type.";
    private const string NullElementError = "The field {0} has a null element at {1}.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
        if (value == null)
            return ValidationResult.Success;

        var name = validationContext.DisplayName;

        if (value is string str)
            return IsValid(str.Length, name);

        if (value is not IEnumerable enumerable)
            return new ValidationResult(string.Format(InvalidValueType, name, value.GetType()));

        if (AllowNullElement && TryGetCount(value, out var count))
            return IsValid(count.Value, name);

        var index = 0;
        foreach (var item in enumerable)
        {
            if (AllowNullElement == false && item is null)
                return new ValidationResult(string.Format(NullElementError, name, index));

            index++;

            if (AllowNullElement && index >= MinLength)
                break;
        }

        return IsValid(index, name);
    }

    private ValidationResult? IsValid(int count, string name)
    {
        return count < MinLength
            ? new ValidationResult(string.Format(MinLengthError, name, MinLength))
            : ValidationResult.Success;
    }

    private static bool TryGetCount(object value, [NotNullWhen(true)] out int? count)
    {
        if (value is ICollection collection)
        {
            count = collection.Count;
            return true;
        }

        var member = value.GetType().GetDataMember(nameof(ICollection.Count));
        if (member != null && member.CanRead && member.DataMemberType == typeof(int))
        {
            count = (int)member.GetValue(value)!;
            return true;
        }

        count = -1;
        return false;
    }
}
