namespace FclEx.Cooperation.Atlassian;

public class JiraTests
{
    [Fact]
    public async Task GetProjectAsync_Test()
    {
        var project = await JiraApi.Projects.GetProjectAsync("FCL");
        Assert.NotNull(project);
    }

    [Fact]
    public async Task GetProjectsAsync_Test()
    {
        var projects = await JiraApi.Projects.GetProjectsAsync().Continue(m => m.AsIReadOnlyList());
        Assert.NotEmpty(projects);
    }

    [Fact]
    public async Task GetIssueTypesForProjectAsync_Test()
    {
        var types = await JiraApi.IssueTypes.GetIssueTypesForProjectAsync("FCL");
        Assert.NotEmpty(types);
    }

    [Fact]
    public async Task GetIssueAsync_Test()
    {
        var issue = await JiraApi.Issues.GetIssueAsync("FCL-1");
        Assert.NotNull(issue);
        AssertExt.NotEmpty(issue.Description);
    }

    [Fact]
    public async Task UpdateIssueAsync_Test()
    {
        var issue = await JiraApi.Issues.GetIssueAsync("FCL-1");
        issue.Description = "description";
        await issue.SaveChangesAsync();
    }

    [Fact]
    public async Task Queryable_ToArray_Test()
    {
        var types = await JiraApi.IssueTypes.GetIssueTypesForProjectAsync("FCL");
        var type = types.First(m => m.Name == "Task");
        var issues = JiraApi.Issues.Queryable
            .Where(m => m.Type == type.Name)
            .ToArray();

        Assert.NotEmpty(issues);
        foreach (var issue in issues)
        {
            Assert.Equal(issue.Type.Id, type.Id);
        }
    }
}