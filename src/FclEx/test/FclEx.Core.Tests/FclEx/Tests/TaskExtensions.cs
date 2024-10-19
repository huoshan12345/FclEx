namespace FclEx.Tests;

public static class TaskExtensions
{
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