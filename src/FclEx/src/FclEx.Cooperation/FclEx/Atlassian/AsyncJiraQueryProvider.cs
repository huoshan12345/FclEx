using Atlassian.Jira;
using Atlassian.Jira.Linq;

namespace FclEx.Atlassian;

public class AsyncJiraQueryProvider : IAsyncQueryProvider
{
    private readonly IJqlExpressionVisitor _translator;
    private readonly IIssueService _issues;

    public AsyncJiraQueryProvider(IJqlExpressionVisitor translator, IIssueService issues)
    {
        _translator = translator;
        _issues = issues;
    }

    public IAsyncQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new AsyncJiraQueryable<TElement>(this, expression);
    }

    private Task<IPagedQueryResult<Issue>> GetList(Expression expression, CancellationToken cancellationToken = default)
    {
        var jql = _translator.Process(expression);
        return _issues.GetIssuesFromJqlAsync(jql.Expression, jql.NumberOfResults, jql.SkipResults ?? 0, cancellationToken);
    }

    public async IAsyncEnumerator<TResult> GetAsyncEnumerator<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var list = await GetList(expression, cancellationToken);
        foreach (var item in list)
        {
            yield return item.CastTo<TResult>()!;
        }
    }

#pragma warning disable CS1998
    public async ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
#pragma warning restore CS1998
    {
        var list = await GetList(expression, cancellationToken);
        var queryable = list.ToAsyncEnumerable().AsAsyncQueryable();
        var treeCopier = new IssueExpressionVisitor(queryable);
        var newExpressionTree = treeCopier.Visit(expression)!;
        var obj = await queryable.Provider.ExecuteAsync<TResult>(newExpressionTree, cancellationToken);
        return obj.CastTo<TResult>()!;
    }
}