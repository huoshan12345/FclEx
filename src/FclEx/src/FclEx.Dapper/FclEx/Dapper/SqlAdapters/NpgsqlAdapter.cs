namespace FclEx.Dapper.SqlAdapters;

public class NpgsqlAdapter : AbstractSqlAdapter<NpgsqlAdapter>
{
    public override string SelectIdentitySql { get; } = "SELECT LASTVAL()";

    protected override QuotationMarks QuotationMarks { get; } = new('"');

    public override DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        return _creater.Value.Invoke(name, value, type);
    }

    private static readonly Lazy<DbParameterCreater> _creater = new(BuildParameterCreater, true);

    private static DbParameterCreater BuildParameterCreater()
    {
        var type = Type.GetType("Npgsql.NpgsqlParameter, Npgsql", true)!;
        var ctor = type.GetRequiredConstructor(typeof(string), typeof(object));

        var paraOfName = Expression.Parameter(typeof(string));
        var paraOfValue = Expression.Parameter(typeof(object));
        var paraOfType = Expression.Parameter(typeof(string));

        var obj = Expression.New(ctor, paraOfName, paraOfValue);
        var result = Expression.Variable(type, "result");
        var exps = new List<Expression>
        {
            Expression.Assign(result, obj)
        };

        var propOfDbType = type.GetRequiredProperty("NpgsqlDbType");
        var methodOfEnumParse = typeof(Enum).GetRequiredMethod(nameof(Enum.Parse), 0, typeof(Type), typeof(string), typeof(bool));
        var enumParse = Expression.Call(null, methodOfEnumParse, new Expression[] { Expression.Constant(propOfDbType.PropertyType), paraOfType, Expression.Constant(true) });
        var convert = Expression.Convert(enumParse, propOfDbType.PropertyType);
        var property = Expression.Property(result, propOfDbType);
        var assignExp = Expression.Assign(property, convert);
        var nullCheck = Expression.ReferenceNotEqual(paraOfType, Expression.Constant(null, typeof(string)));
        var ifThen = Expression.IfThen(nullCheck, assignExp);
        exps.Add(ifThen);
        exps.Add(result);
        var final = Expression.Block(new[] { result }, exps);
        return Expression.Lambda<DbParameterCreater>(final, paraOfName, paraOfValue, paraOfType).Compile();
    }

}