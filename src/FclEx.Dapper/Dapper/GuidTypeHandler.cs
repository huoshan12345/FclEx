namespace Dapper;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    private static readonly Type _type = typeof(Guid);

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

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value;
        parameter.DbType = DbType.Guid;
    }
}