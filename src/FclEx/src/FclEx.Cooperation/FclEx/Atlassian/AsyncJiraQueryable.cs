namespace FclEx.Atlassian;

public class AsyncJiraQueryable<T> : IAsyncQueryable<T>
{
    private readonly AsyncJiraQueryProvider _provider;

    public AsyncJiraQueryable(AsyncJiraQueryProvider provider, Expression? expression = null)
    {
        _provider = provider;
        Expression = expression ?? Expression.Constant(this);
    }

    public Type ElementType { get; } = typeof(T);
    public Expression Expression { get; }
    public IAsyncQueryProvider Provider => _provider;
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return _provider.GetAsyncEnumerator<T>(Expression, cancellationToken);
    }
}