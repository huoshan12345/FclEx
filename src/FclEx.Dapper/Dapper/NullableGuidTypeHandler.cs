namespace Dapper;

public class NullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
{
    private static readonly Type _type = typeof(Guid?);

    public override Guid? Parse(object value)
    {
        return value switch
        {
            null or DBNull => null,
            Guid guid => guid,
            string str => Guid.Parse(str),
            _ => throw new InvalidCastException($"Invalid cast from '{value.GetType().FullName}' to {_type.FullName}."),
        };
    }

    public override void SetValue(IDbDataParameter parameter, Guid? value)
    {
        parameter.Value = value;
        parameter.DbType = DbType.Guid;
    }
}