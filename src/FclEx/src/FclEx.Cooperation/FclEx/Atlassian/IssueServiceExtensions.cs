using Atlassian.Jira;
using Atlassian.Jira.Linq;

namespace FclEx.Atlassian;

public static class IssueServiceExtensions
{
    private static readonly Type _typeOfIssueService = typeof(IIssueService).Assembly
        .GetTypes()
        .Single(t => t.IsAssignableTo(typeof(IIssueService)) && t.IsAbstract == false);

    private static readonly FieldInfo _fieldOfJira = _typeOfIssueService.GetField("_jira", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static IAsyncQueryable<Issue> AsyncQueryable(this IIssueService service)
    {
        var jira = _fieldOfJira.GetValue(service).CastTo<Jira>()!;
        var translator = jira.Services.Get<IJqlExpressionVisitor>();
        var provider = new AsyncJiraQueryProvider(translator, service);
        return new AsyncJiraQueryable<Issue>(provider);
    }
}