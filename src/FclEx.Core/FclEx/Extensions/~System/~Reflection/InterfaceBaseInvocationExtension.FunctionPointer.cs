namespace FclEx.Extensions;

partial class InterfaceBaseInvocationExtension
{
    private static readonly ConditionalWeakTable<InterfaceMethodInfo, Tuple<IntPtr, MethodInfo>> MethodMap = new();

    internal static void BaseByFunctionPointer<TInterface>(this TInterface instance, Expression<Action<TInterface>> selector)
    {
        var (invoke, invoker, args) = instance.GetInterfaceFunc(selector);
        invoke.Invoke(invoker, args);
    }

    internal static TReturn BaseByFunctionPointer<TInterface, TReturn>(this TInterface instance, Expression<Func<TInterface, TReturn>> selector)
    {
        var (invoke, invoker, args) = instance.GetInterfaceFunc(selector);
        return invoke.Invoke(invoker, args).CastTo<TReturn>()!;
    }

    private static (IntPtr pointer, MethodInfo invoke) GetInterfaceMethodDelegate(InterfaceMethodInfo info)
    {
        var (interfaceMethod, paraTypes) = GetInterfaceMethod(info);
        var delegateType = DelegateHelper.MakeNewCustomDelegate(info.Method.ReturnType, paraTypes);
        var pointer = interfaceMethod.MethodHandle.GetFunctionPointer();
        return (pointer, delegateType.GetMethod(nameof(Action.Invoke))!);
    }

    private static (MethodInfo invoke, object invoker, object?[] args) GetInterfaceFunc<TInterface>(this TInterface instance, LambdaExpression selector)
    {
        Check.NotNull(instance);
        Check.NotNull(selector);

        var (method, args) = GetMethodAndArguments(selector);
        var evaluatedArguments = args.Select(m => m.Evaluate()).ToArray();
        var interfaceType = typeof(TInterface);
        var (pointer, invoke) = MethodMap.GetValue(new(instance.GetType(), interfaceType, method), m => GetInterfaceMethodDelegate(m).ToTuple());
        var invoker = Activator.CreateInstance(invoke.DeclaringType!, instance, pointer);
        return (invoke, invoker!, evaluatedArguments);
    }
}
