namespace FclEx.Utils;

public static partial class Operation
{
    public static OperationResult Execute(Action action)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            action();
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static OperationResult<T> Execute<T>(Func<T> action)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = action();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static OperationResult Execute(Func<OperationResult> action) => Execute<OperationResult>(action).Unwrap();

    public static OperationResult<T> Execute<T>(Func<OperationResult<T>> action) => Execute<OperationResult<T>>(action).Unwrap();
}