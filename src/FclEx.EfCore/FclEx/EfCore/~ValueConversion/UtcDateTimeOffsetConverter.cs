namespace FclEx.EfCore;

/// <summary>Normalizes DateTimeOffset values to UTC when writing provider values.</summary>
/// <remarks>The instant is preserved and the written offset is zero. Values read from the provider are returned unchanged.
/// EF Core handles null values without invoking this converter.</remarks>
public class UtcDateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    /// <summary>Creates a converter that normalizes writes to UTC and leaves reads unchanged.</summary>
    public UtcDateTimeOffsetConverter()
        : base(v => v.ToUniversalTime(), v => v) { }
}
