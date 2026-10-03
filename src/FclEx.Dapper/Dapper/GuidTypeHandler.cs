namespace Dapper;

/// <summary>
/// Reads GUID values from provider GUID values, strings, or 16-byte arrays and binds Oracle GUID parameters as binary values.
/// </summary>
/// <remarks>
/// The default registration performed by <see cref="DapperHelper.Initialize"/> changes
/// Dapper's process-wide type-handler state. Database nulls are rejected rather than converted to
/// <see cref="Guid.Empty"/>.
/// </remarks>
public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    private static readonly Type _type = typeof(Guid);

    /// <summary>
    /// Converts a provider GUID value, its string representation, or its 16-byte representation to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The non-null database value to convert.</param>
    /// <returns>The converted GUID.</returns>
    /// <exception cref="InvalidCastException">
    /// <paramref name="value"/> is null, <see cref="DBNull.Value"/>, or neither a GUID, a string, nor a byte array.
    /// </exception>
    /// <exception cref="FormatException">A string value is not a valid GUID.</exception>
    public override Guid Parse(object value)
    {
        return value switch
        {
            null or DBNull => throw new InvalidCastException($"Invalid cast from null to '{_type.FullName}'."),
            Guid guid => guid,
            byte[] bytes => new Guid(bytes),
            string str => Guid.Parse(str),
            _ => throw new InvalidCastException($"Invalid cast from '{value.GetType().FullName}' to {_type.FullName}."),
        };
    }

    /// <summary>
    /// Binds Oracle parameters as GUID bytes with <see cref="DbType.Binary"/>; other providers receive the GUID with <see cref="DbType.Guid"/>.
    /// </summary>
    /// <param name="parameter">The parameter to configure.</param>
    /// <param name="value">The GUID value to assign.</param>
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        for (var type = parameter.GetType(); type is not null; type = type.BaseType)
        {
            if (type.FullName == "Oracle.ManagedDataAccess.Client.OracleParameter"
                && type.Assembly.GetName().Name == "Oracle.ManagedDataAccess")
            {
                parameter.Value = value.ToByteArray();
                parameter.DbType = DbType.Binary;
                return;
            }
        }

        parameter.Value = value;
        parameter.DbType = DbType.Guid;
    }
}
