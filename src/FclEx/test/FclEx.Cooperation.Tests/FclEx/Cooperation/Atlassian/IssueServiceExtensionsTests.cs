using FclEx.Atlassian;

namespace FclEx.Cooperation.Atlassian;

public class IssueServiceExtensionsTests
{
    [Fact]
    public async Task AsyncQueryable_FirstOrDefaultAsync_Test()
    {
        var issue = await JiraApi.Issues.GetIssueAsync("FCL-1");
        var issueByQuery = await JiraApi.Issues.AsyncQueryable()
            .Where(m => m.Summary == issue.Summary)
            .FirstOrDefaultAsync();

        Assert.NotNull(issueByQuery);
        Assert.Equal(issue.Key, issueByQuery.Key);
    }

    [Fact]
    public async Task AsyncQueryable_ToArrayAsync_Test()
    {
        var types = await JiraApi.IssueTypes.GetIssueTypesForProjectAsync("FCL");
        var story = types.First(m => m.Name == "Task");
        var issues = await JiraApi.Issues.AsyncQueryable()
            .Where(m => m.Type == story.Name)
            .ToArrayAsync();

        Assert.NotEmpty(issues);
        foreach (var issue in issues)
        {
            Assert.Equal(issue.Type.Id, story.Id);
        }
    }

    [Fact]
    public async Task AsyncQueryable_Foreach_Test()
    {
        var types = await JiraApi.IssueTypes.GetIssueTypesForProjectAsync("FCL");
        var story = types.First(m => m.Name == "Task");
        var queryable = JiraApi.Issues.AsyncQueryable()
            .Where(m => m.Type == story.Name);

        await foreach (var issue in queryable)
        {
            Assert.Equal(issue.Type.Id, story.Id);
        }
    }
}