namespace FclEx.Helpers;

public static partial class Method
{
    public static MethodInfo Of(Action action) => action.Method;
    public static MethodInfo Of<TResult>(Func<TResult> func) => func.Method;
}