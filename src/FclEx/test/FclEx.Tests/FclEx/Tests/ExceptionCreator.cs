namespace FclEx.Tests;

public static class ExceptionCreator
{
    public static Task Run()
    {
        var tasks = Enumerable.Range(1, 2).Select(m => D(m));
        return tasks.WhenAllOrAllErrors();
    }

    private static async Task A(int index)
    {
        await Task.Yield();
        throw new Exception(nameof(A) + index);
    }

    private static async Task B(int index)
    {
        try
        {
            await A(index);
        }
        catch (Exception ex)
        {
            if (index % 2 == 0)
                throw;
            else
                throw new InvalidOperationException(nameof(A) + index, ex);
        }
    }

    private static Task C(int count)
    {
        var tasks = Enumerable.Range(1, count).Select(m => B(m));
        return tasks.WhenAllOrAllErrors();
    }

    private static async Task D(int index)
    {
        try
        {
            await C(index);
        }
        catch (Exception ex)
        {
            if (index % 2 == 0)
                throw;
            else
                throw new InvalidOperationException(nameof(D) + index, ex);
        }
    }
}