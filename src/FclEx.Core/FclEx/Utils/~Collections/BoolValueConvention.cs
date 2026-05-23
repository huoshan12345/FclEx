namespace FclEx.Utils;

/// <summary>
/// Specifies how a boolean value should be converted to a query parameter.
/// </summary>
public enum BoolValueConvention
{
    /// <summary>
    /// The conversion behavior is not explicitly set and should be determined by external logic or defaults.
    /// </summary>
    Unset = 0,
    /// <summary>
    /// Convert the boolean value to its string representation ("True" or "False").
    /// </summary>
    AsString,
    /// <summary>
    /// Convert the boolean value to its lowercase string representation ("true" or "false").
    /// </summary>
    AsLowercase,
    /// <summary>
    /// Convert the boolean value to a numeric representation (True -> 1, False -> 0).
    /// </summary>
    AsNumber,
}