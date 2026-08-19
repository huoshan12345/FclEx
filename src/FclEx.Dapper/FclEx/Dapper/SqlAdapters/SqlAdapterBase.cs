namespace FclEx.Dapper.SqlAdapters;

public readonly record struct QuotationMarks(char Prefix, char Suffix)
{
    public QuotationMarks(char mark) : this(mark, mark) { }
}

public abstract class SqlAdapterBase<TSelf> : ISqlAdapter where TSelf : SqlAdapterBase<TSelf>, new()
{
    protected readonly Lazy<DbParameterCreator> _creator;

    protected SqlAdapterBase()
    {
        _creator = new(BuildParameterCreator, true);
    }

    public static readonly TSelf Instance = new();

    public virtual bool SupportSchema { get; } = true;
    public abstract string SelectIdentitySql { get; }

    protected abstract QuotationMarks QuotationMarks { get; }

    // ReSharper disable once VirtualMemberNeverOverridden.Global
    protected virtual string GetQuotedName(string name)
    {
        var (prefix, suffix) = QuotationMarks;
        return StringBuilder.Build(m => m.Append(prefix).Append(name).Append(suffix));
    }

    public virtual string GetQuotedTableName(string name)
    {
        return GetQuotedName(name);
    }

    public virtual string GetQuotedColumnName(string name)
    {
        return GetQuotedName(name);
    }

    protected abstract DbParameterCreator BuildParameterCreator();

    public virtual DbParameter CreateParameter(string name, object? value, string? type = null)
    {
        value ??= DBNull.Value;
        return _creator.Value.Invoke(name, value, type);
    }

    public virtual ValueTask<IAsyncDisposable> EnableIdentityInsertAsync<T>(string? schema, IDbCommand cmd)
    {
        return AsyncDisposable.EmptyValueTask;
    }

    protected static DbParameterCreator BuildParameterCreator(string typeName, string dbTypePropName)
    {
        var type = Type.GetType(typeName, true)!;
        var ctor = type.GetRequiredConstructor(typeof(string), typeof(object));

        var paraOfName = Expression.Parameter(typeof(string));
        var paraOfValue = Expression.Parameter(typeof(object));
        var paraOfType = Expression.Parameter(typeof(string));

        var obj = Expression.New(ctor, paraOfName, paraOfValue);
        var result = Expression.Variable(type, "result");
        var expList = new List<Expression>
        {
            Expression.Assign(result, obj)
        };

        var propOfDbType = type.GetRequiredProperty(dbTypePropName);
        var methodOfEnumParse = typeof(Enum).GetRequiredMethod(nameof(Enum.Parse), 0, typeof(Type), typeof(string), typeof(bool));
        var enumParse = Expression.Call(null, methodOfEnumParse, Expression.Constant(propOfDbType.PropertyType), paraOfType, Expression.Constant(true));
        var convert = Expression.Convert(enumParse, propOfDbType.PropertyType);
        var property = Expression.Property(result, propOfDbType);
        var assignExp = Expression.Assign(property, convert);
        var nullCheck = Expression.ReferenceNotEqual(paraOfType, Expression.Constant(null, typeof(string)));
        var ifThen = Expression.IfThen(nullCheck, assignExp);
        expList.Add(ifThen);
        expList.Add(result);
        var final = Expression.Block([result], expList);
#if DEBUG
        // final.Enumerate().ForEach(e => Console.WriteLine(e.ToString()));
#endif
        return Expression.Lambda<DbParameterCreator>(final, paraOfName, paraOfValue, paraOfType).Compile();
    }
}