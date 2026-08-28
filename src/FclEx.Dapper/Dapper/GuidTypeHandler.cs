namespace Dapper;

/// <summary>
/// Provides an opt-in Dapper type handler that reads <see cref="Guid"/> values from provider GUID values or strings.
/// </summary>
/// <remarks>
/// Registering this handler through <see cref="SqlMapper.AddTypeHandler{T}(SqlMapper.TypeHandler{T})"/> changes
/// Dapper's process-wide type-handler state. Database nulls are rejected rather than converted to
/// <see cref="Guid.Empty"/>.
/// </remarks>
public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    private static readonly Type _type = typeof(Guid);

    /// <summary>
    /// Converts a provider GUID value or its string representation to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The non-null database value to convert.</param>
    /// <returns>The converted GUID.</returns>
    /// <exception cref="InvalidCastException">
    /// <paramref name="value"/> is null, <see cref="DBNull.Value"/>, or neither a GUID nor a string.
    /// </exception>
    /// <exception cref="FormatException">A string value is not a valid GUID.</exception>
    public override Guid Parse(object value)
    {
        return value switch
        {
            null or DBNull => throw new InvalidCastException($"Invalid cast from null to '{_type.FullName}'."),
            Guid guid => guid,
            string str => Guid.Parse(str),
            _ => throw new InvalidCastException($"Invalid cast from '{value.GetType().FullName}' to {_type.FullName}."),
        };
    }

    /// <summary>
    /// Assigns a GUID value to a database parameter and sets its type to <see cref="DbType.Guid"/>.
    /// </summary>
    /// <param name="parameter">The parameter to configure.</param>
    /// <param name="value">The GUID value to assign.</param>
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value;
        parameter.DbType = DbType.Guid;
    }
}
