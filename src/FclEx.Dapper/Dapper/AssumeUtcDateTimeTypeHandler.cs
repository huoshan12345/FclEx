namespace Dapper;

public class AssumeUtcDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value.AssumeUtc();
    }

    public override DateTime Parse(object value)
    {
        var time = (DateTime)value;
        return time.AssumeUtc();
    }
}