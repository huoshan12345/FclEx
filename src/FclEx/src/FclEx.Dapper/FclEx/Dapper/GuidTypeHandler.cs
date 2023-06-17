namespace FclEx.Dapper;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return value switch
        {
            null => Guid.Empty,
            Guid guid => guid,
            string str => Guid.Parse(str),
            _ => throw new InvalidCastException($"Invalid cast from '{value.GetType().FullName}' to 'System.Guid'.")
        };
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value;
        parameter.DbType = DbType.Guid;
    }
}