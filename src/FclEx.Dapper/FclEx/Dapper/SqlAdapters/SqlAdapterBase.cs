namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Describes the opening and terminating delimiters used to quote identifiers in a SQL dialect.
/// </summary>
/// <param name="Prefix">The opening delimiter.</param>
/// <param name="Suffix">The terminating delimiter, which is doubled when it appears inside an identifier.</param>
public readonly record struct QuotationMarks(char Prefix, char Suffix)
{
    /// <summary>
    /// Creates a delimiter pair that uses the same character to open and terminate an identifier.
    /// </summary>
    /// <param name="mark">The shared opening and terminating delimiter.</param>
    public QuotationMarks(char mark) : this(mark, mark) { }
}

/// <summary>
/// Provides common identifier quoting, INSERT construction, parameter creation, and batch-limit behavior for SQL adapters.
/// </summary>
/// <remarks>
/// Derived adapters supply provider delimiters, parameter construction, and provider-specific limits or generated-key
/// SQL. An adapter's SQL-affecting behavior must remain stable while its instance is registered because command text
/// is cached by adapter identity.
/// </remarks>
public abstract class SqlAdapterBase : ISqlAdapter
{
    /// <summary>
    /// Lazily creates and caches the compiled provider parameter factory for this adapter instance.
    /// </summary>
    protected readonly Lazy<DbParameterCreator> _creator;

    /// <summary>
    /// Initializes an adapter with a thread-safe lazy parameter factory.
    /// </summary>
    protected SqlAdapterBase()
    {
        _creator = new(BuildParameterCreator, true);
    }

    /// <inheritdoc />
    public virtual bool SupportsSchemas { get; } = true;

    /// <summary>
    /// Gets the provider delimiters used for table, schema, and column identifiers.
    /// </summary>
    protected abstract QuotationMarks QuotationMarks { get; }

    // ReSharper disable once VirtualMemberNeverOverridden.Global
    /// <summary>
    /// Quotes one unqualified identifier component and escapes embedded terminating delimiters.
    /// </summary>
    /// <param name="name">The unquoted identifier from trusted application configuration.</param>
    /// <returns>The delimited and escaped identifier.</returns>
    protected virtual string GetQuotedName(string name)
    {
        var (prefix, suffix) = QuotationMarks;

        // Delimited identifiers do not nest, so a prefix inside the name is ordinary content; only the suffix can
        // terminate the identifier and must be doubled. When prefix and suffix are the same character, doubling the
        // suffix naturally handles every occurrence of that shared delimiter.
        var escapedName = name.Replace(suffix.ToString(), new string(suffix, 2));
        return StringBuilder.Build(m => m.Append(prefix).Append(escapedName).Append(suffix));
    }

    /// <inheritdoc />
    public virtual string GetQuotedTableName(string name)
    {
        return GetQuotedName(name);
    }

    /// <inheritdoc />
    public virtual string GetQuotedColumnName(string name)
    {
        return GetQuotedName(name);
    }

    /// <inheritdoc />
    public abstract int GetMaxInsertBatchSize(int parameterCountPerRow);

    /// <inheritdoc />
    /// <remarks>
    /// The base implementation emits <c>DEFAULT VALUES</c> for a default-only row and does not support returning a
    /// generated key. Derived adapters override this method when their default-row or key-returning syntax differs.
    /// </remarks>
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

    /// <summary>
    /// Builds the provider-specific delegate used to create command parameters.
    /// </summary>
    /// <returns>A reusable parameter factory.</returns>
    protected abstract DbParameterCreator BuildParameterCreator();

    /// <summary>
    /// Calculates a provider batch limit from its maximum command parameter count.
    /// </summary>
    /// <param name="parameterCountPerRow">The positive parameter count required for one row.</param>
    /// <param name="maxParameterCount">The positive maximum number of parameters accepted by one command.</param>
    /// <returns>The maximum complete rows that fit in one command.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parameterCountPerRow"/> is not positive.</exception>
    /// <exception cref="NotSupportedException">One row exceeds <paramref name="maxParameterCount"/>.</exception>
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

    /// <inheritdoc />
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

    /// <summary>
    /// Builds a compiled parameter factory for a provider parameter type discovered at runtime.
    /// </summary>
    /// <param name="typeName">The assembly-qualified provider parameter type name.</param>
    /// <param name="dbTypePropName">The provider enum property used for recognized store type names.</param>
    /// <returns>
    /// A factory that invokes the provider parameter's <c>(string, object)</c> constructor and, when recognized,
    /// assigns the requested provider type enum.
    /// </returns>
    /// <remarks>The corresponding provider assembly must be available when the returned factory is first built.</remarks>
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
        var tryParseEnum = typeof(SqlAdapterBase).GetRequiredMethod(nameof(TryParseEnum), 0, typeof(Type), typeof(string));
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
