using Atlassian.Jira;

namespace FclEx.Atlassian;

public class IssueExpressionVisitor : ExpressionVisitor
{
    private readonly IAsyncQueryable<Issue> _queryableIssues;

    public IssueExpressionVisitor(IAsyncQueryable<Issue> queryableIssues)
    {
        _queryableIssues = queryableIssues;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        return node.Type == typeof(AsyncJiraQueryable<Issue>)
            ? Expression.Constant(_queryableIssues)
            : node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (node.Method.Name 
            is nameof(Enumerable.Where) 
            or nameof(Enumerable.Take) 
            or nameof(Enumerable.OrderBy)
            or nameof(Enumerable.OrderByDescending) 
            or nameof(Enumerable.ThenBy) 
            or nameof(Enumerable.ThenByDescending))
        {
            return Expression.Constant(_queryableIssues);
        }

        return base.VisitMethodCall(node);
    }
}