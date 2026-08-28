namespace Dapper;

/// <summary>
/// Provides an opt-in Dapper type handler that normalizes <see cref="DateTime"/> values to UTC.
/// </summary>
/// <remarks>
/// Local values are converted with <see cref="DateTime.ToUniversalTime"/> so they retain the same instant.
/// Unspecified values keep their ticks and are interpreted as UTC; UTC values are unchanged. Registering this
/// handler through <see cref="SqlMapper.AddTypeHandler{T}(SqlMapper.TypeHandler{T})"/> changes Dapper's
/// process-wide type-handler state.
/// </remarks>
public class AssumeUtcDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    /// <summary>
    /// Normalizes a date-time value to UTC and assigns it to a database parameter.
    /// </summary>
    /// <param name="parameter">The parameter to configure.</param>
    /// <param name="value">The date-time value to normalize and assign.</param>
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value.AssumeUtc();
    }

    /// <summary>
    /// Reads a provider <see cref="DateTime"/> value and normalizes it to UTC.
    /// </summary>
    /// <param name="value">The provider value, which must be a <see cref="DateTime"/>.</param>
    /// <returns>The normalized UTC value.</returns>
    /// <exception cref="InvalidCastException"><paramref name="value"/> is not a <see cref="DateTime"/>.</exception>
    public override DateTime Parse(object value)
    {
        var time = (DateTime)value;
        return time.AssumeUtc();
    }
}
