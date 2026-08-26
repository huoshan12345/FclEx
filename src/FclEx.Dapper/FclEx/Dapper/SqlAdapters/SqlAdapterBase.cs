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

    public virtual bool SupportsSchemas { get; } = true;

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

    public abstract int GetMaxInsertBatchSize(int parameterCountPerRow);

    public virtual string BuildInsertCommandText(
        string quotedTableName,
        string? columnListSql,
        string? valueRowsSql,
        string? quotedGeneratedKeyColumn)
    {
        if ((columnListSql is null) != (valueRowsSql is null))
            throw new ArgumentException("The column list and value rows must either both be supplied or both be null.");
        if (quotedGeneratedKeyColumn is not null)
            throw new NotSupportedException($"'{GetType().FullName}' does not support generated-key retrieval.");

        return columnListSql is null
            ? $"INSERT INTO {quotedTableName}{Environment.NewLine}DEFAULT VALUES"
            : $"INSERT INTO {quotedTableName} ({columnListSql}){Environment.NewLine}VALUES{Environment.NewLine}{valueRowsSql}";
    }

    protected abstract DbParameterCreator BuildParameterCreator();

    protected static int CalculateMaxInsertBatchSize(int parameterCountPerRow, int maxParameterCount)
    {
        if (parameterCountPerRow <= 0)
            throw new ArgumentOutOfRangeException(nameof(parameterCountPerRow));

        var rowCount = maxParameterCount / parameterCountPerRow;
        if (rowCount == 0)
        {
            throw new NotSupportedException(
                $"A row requiring {parameterCountPerRow} parameters exceeds the limit of {maxParameterCount} parameters per command.");
        }

        return rowCount;
    }

    public virtual DbParameter CreateParameter(string name, object? value, string? storeTypeName = null)
    {
        value ??= DBNull.Value;
        return _creator.Value.Invoke(name, value, storeTypeName);
    }

    /// <inheritdoc />
    public virtual ValueTask<IAsyncDisposable> BeginExplicitIdentityInsertAsync(
        string quotedTableName,
        DbCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return AsyncDisposable.EmptyValueTask;
    }

    protected static DbParameterCreator BuildParameterCreator(string typeName, string dbTypePropName)
    {
        var type = Type.GetType(typeName, true)!;
        var ctor = type.GetRequiredConstructor(typeof(string), typeof(object));

        var paraOfName = Expression.Parameter(typeof(string));
        var paraOfValue = Expression.Parameter(typeof(object));
        var parameterOfStoreTypeName = Expression.Parameter(typeof(string));

        var obj = Expression.New(ctor, paraOfName, paraOfValue);
        var result = Expression.Variable(type, "result");
        var expList = new List<Expression>
        {
            Expression.Assign(result, obj)
        };

        var propOfDbType = type.GetRequiredProperty(dbTypePropName);
        var parsedType = Expression.Variable(typeof(object), "parsedType");
        var tryParseEnum = typeof(SqlAdapterBase<TSelf>).GetRequiredMethod(nameof(TryParseEnum), 0, typeof(Type), typeof(string));
        var parse = Expression.Assign(parsedType,
            Expression.Call(null, tryParseEnum, Expression.Constant(propOfDbType.PropertyType), parameterOfStoreTypeName));
        expList.Add(parse);
        var convert = Expression.Convert(parsedType, propOfDbType.PropertyType);
        var property = Expression.Property(result, propOfDbType);
        var assignExp = Expression.Assign(property, convert);
        var nullCheck = Expression.ReferenceNotEqual(parsedType, Expression.Constant(null, typeof(object)));
        var ifThen = Expression.IfThen(nullCheck, assignExp);
        expList.Add(ifThen);
        expList.Add(result);
        var final = Expression.Block([result, parsedType], expList);
#if DEBUG
        // final.Enumerate().ForEach(e => Console.WriteLine(e.ToString()));
#endif
        return Expression.Lambda<DbParameterCreator>(final, paraOfName, paraOfValue, parameterOfStoreTypeName).Compile();
    }

    private static object? TryParseEnum(Type enumType, string? value)
    {
        if (value is null)
            return null;

        try
        {
            return Enum.Parse(enumType, value, true);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
