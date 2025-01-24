namespace FclEx.Dapper;

public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value;
    }

    public override DateTime Parse(object value)
    {
        var t = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        return t;

    }
}