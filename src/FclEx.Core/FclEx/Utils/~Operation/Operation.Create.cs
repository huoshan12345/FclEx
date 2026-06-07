namespace FclEx.Utils;

public partial class Operation
{
    public static OperationResult NotImplemented() => NotImplemented<Unit>();

    public static OperationResult Cancel(Exception ex, TimeSpan elapsed = default) => Cancel<Unit>(ex, elapsed);

    public static OperationResult Cancel(TimeSpan elapsed = default) => Cancel<Unit>(elapsed);

    public static OperationResult Success(TimeSpan elapsed = default) => Success<Unit>(default, elapsed);

    public static OperationResult Error(Exception ex, TimeSpan elapsed = default) => Error<Unit>(ex, elapsed);

    public static OperationResult Error(string error, TimeSpan elapsed = default) => Error<Unit>(error, elapsed);
}