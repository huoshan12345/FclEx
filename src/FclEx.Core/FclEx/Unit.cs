namespace FclEx;

/// <summary>
/// Represents the absence of a meaningful value when an API shape requires a value type.
/// </summary>
/// <remarks>
/// JSON serialization uses <see cref="IgnoreJsonConverter"/> so payloads for this marker type are ignored.<br/>
/// Nested values are written as JSON <see langword="null"/>, and any JSON value read for this type becomes
/// <see cref="Default"/>.
/// </remarks>
[JsonConverter(typeof(IgnoreJsonConverter))]
public readonly record struct Unit
{
    /// <summary>
    /// Gets the single semantic value of <see cref="Unit"/>.
    /// </summary>
    public static readonly Unit Default = default;

    public override int GetHashCode() => 0;
    public override string ToString() => "()";
}
