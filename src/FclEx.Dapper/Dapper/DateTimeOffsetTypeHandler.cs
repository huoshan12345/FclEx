using System.Globalization;

namespace Dapper;

/// <summary>
/// Reads date-time offsets from native values, date-time values, and invariant text such as SQLite TEXT values.
/// </summary>
/// <remarks>
/// Native offsets and explicit offsets in text are preserved. Unspecified <see cref="DateTime"/> values
/// and text without an offset are interpreted as UTC; local date-time values retain their instant.
/// The default registration by <see cref="DapperHelper.Initialize"/> changes Dapper's process-wide
/// type-handler state and preserves a handler already registered by the application.
/// </remarks>
public class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>, SqlMapper.ITypeHandler
{
    /// <summary>Converts a native date-time offset, a date-time value, or invariant date-time text.</summary>
    /// <param name="value">The non-null database value.</param>
    /// <returns>The converted value, preserving explicit offsets and available tick precision.</returns>
    /// <exception cref="InvalidCastException">The value is null, a database null, or an unsupported type.</exception>
    /// <exception cref="FormatException">A string does not represent a valid date-time offset.</exception>
    public override DateTimeOffset Parse(object value) => value switch
    {
        DateTimeOffset offset => offset,
        DateTime time => new DateTimeOffset(time.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(time, DateTimeKind.Utc)
            : time),
        string text => DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
        null or DBNull => throw new InvalidCastException("Cannot convert a database null to DateTimeOffset."),
        _ => throw new InvalidCastException($"Cannot convert '{value.GetType().FullName}' to DateTimeOffset."),
    };

    /// <summary>Assigns a native date-time offset parameter without changing its offset.</summary>
    /// <param name="parameter">The provider parameter to configure.</param>
    /// <param name="value">The value to bind.</param>
    /// <remarks>The provider determines its database representation and supported parameter types.</remarks>
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.Value = value;
        parameter.DbType = DbType.DateTimeOffset;
    }

    object? SqlMapper.ITypeHandler.Parse(Type destinationType, object value)
    {
        // Dapper also uses this interface for nullable root scalars. The base handler forwards
        // DBNull to the non-nullable Parse method, so preserve nullable results explicitly.
        if ((value is null or DBNull) && Nullable.GetUnderlyingType(destinationType) == typeof(DateTimeOffset))
            return null;
        return Parse(value!);
    }

    void SqlMapper.ITypeHandler.SetValue(IDbDataParameter parameter, object value)
    {
        if (value is null or DBNull)
            parameter.Value = DBNull.Value;
        else
            SetValue(parameter, (DateTimeOffset)value);
    }
}
