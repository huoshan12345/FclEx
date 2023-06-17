namespace FclEx.Dapper.SqlAdapters;

public readonly record struct QuotationMarks(char Prefix, char Suffix)
{
    public QuotationMarks(char mark) : this(mark, mark) { }
}

public abstract class AbstractSqlAdapter<TSelf> : ISqlAdapter where TSelf : AbstractSqlAdapter<TSelf>, new()
{
    protected readonly Lazy<DbParameterCreater> _creater;

    protected AbstractSqlAdapter()
    {
        _creater = new(BuildParameterCreater, true);
    }

    public static readonly TSelf Instance = new();

    public virtual bool SupportSchema { get; } = true;
    public abstract string SelectIdentitySql { get; }

    protected abstract QuotationMarks QuotationMarks { get; }

    // ReSharper disable once VirtualMemberNeverOverridden.Global
    protected virtual string GetQuotedName(string name)
    {
        var (prefix, suffix) = QuotationMarks;
        return StringBuilderHelper.Build(m => m.Append(prefix).Append(name).Append(suffix));
    }

    public virtual string GetQuotedTableName(string name)
    {
        return GetQuotedName(name);
    }

    public virtual string GetQuotedColumnName(string name)
    {
        return GetQuotedName(name);
    }

    protected abstract DbParameterCreater BuildParameterCreater();

    public virtual DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        value ??= DBNull.Value;
        return _creater.Value.Invoke(name, value, type);
    }

    public virtual Task<IAsyncDisposable> EnableIdentityInsertAsync<T>(string? schema, IDbCommand cmd)
    {
        return AsyncDisposable.EmptyTask;
    }

    protected static DbParameterCreater BuildParameterCreater(string typeName, string dbTypePropName)
    {
        var type = Type.GetType(typeName, true)!;
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

        var propOfDbType = type.GetRequiredProperty(dbTypePropName);
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
#if DEBUG
        final.Visit(e => Console.WriteLine(e.ToString()));
#endif
        return Expression.Lambda<DbParameterCreater>(final, paraOfName, paraOfValue, paraOfType).Compile();
    }
}