namespace FclEx.Tests;

public static class TaskExtensions
{
    // https://stackoverflow.com/questions/12007781/why-doesnt-await-on-task-whenall-throw-an-aggregateexception
    // This solution gives you the aggregate exception (i.e. all the exceptions that were thrown by the various tasks) and doesn't block (workflow is still asynchronous).
    public static async Task WhenAllOrAllErrors(this IEnumerable<Task> tasks)
    {
        var task = Task.WhenAll(tasks);
        try
        {
            await task;
        }
        catch (Exception)
        {
            if (task.Exception is { } ex)
            {
                throw ex;
            }
            else
            {
                throw;
            }
        }
    }
}