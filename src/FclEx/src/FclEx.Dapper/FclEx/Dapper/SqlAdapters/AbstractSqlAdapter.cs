namespace FclEx.Dapper.SqlAdapters;

public readonly record struct QuotationMarks(char Prefix, char Suffix)
{
    public QuotationMarks(char mark) : this(mark, mark) { }
}

public abstract class AbstractSqlAdapter<TSelf> : ISqlAdapter where TSelf : AbstractSqlAdapter<TSelf>, new()
{
    public static readonly TSelf Instance = new();

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

    public abstract DbParameter CreateParameter(string name, object? value, string? type = null);

    public virtual Task<IAsyncDisposable> EnableIdentityInsertAsync<T>(string schema, IDbCommand cmd)
    {
        return AsyncDisposable.EmptyTask;
    }
}