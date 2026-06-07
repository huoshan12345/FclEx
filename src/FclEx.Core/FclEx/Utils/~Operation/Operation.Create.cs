namespace FclEx.Utils;

public static partial class Operation
{
    public static OperationResult NotImplemented() => NotImplemented<Unit>();

    public static OperationResult Cancel(Exception exception, TimeSpan elapsed = default) => Cancel<Unit>(exception, elapsed);

    public static OperationResult Cancel(TimeSpan elapsed = default) => Cancel<Unit>(elapsed);

    public static OperationResult Success(TimeSpan elapsed = default) => Success<Unit>(default, elapsed);

    public static OperationResult Error(Exception exception, TimeSpan elapsed = default) => Error<Unit>(exception, elapsed);

    public static OperationResult Error(string error, TimeSpan elapsed = default) => Error<Unit>(error, elapsed);
}
