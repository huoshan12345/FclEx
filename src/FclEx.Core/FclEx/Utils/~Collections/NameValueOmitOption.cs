namespace FclEx.Utils;

/// <summary>
/// Specifies conditions for omitting a uri parameter during serialization or processing.
/// </summary>
[Flags]
public enum NameValueOmitOption
{
    /// <summary>
    /// The omission behavior is not explicitly set and should be determined by external logic.
    /// </summary>
    Unset = 0,
    /// <summary>
    /// Do not omit the uri parameter regardless of its value.
    /// </summary>
    Never = 1,
    /// <summary>
    /// Omit the uri parameter when the value is <see langword="null"/>.
    /// </summary>
    WhenNull = 1 << 1,
    /// <summary>
    /// Omit the uri parameter when the value is <see langword="null"/> or an empty string or collection.
    /// </summary>
    WhenEmpty = 1 << 2,
    /// <summary>
    /// Omit the uri parameter when the value is <see langword="null"/> or its default value.
    /// </summary>
    WhenDefault = 1 << 3,
}