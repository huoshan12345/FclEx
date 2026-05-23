namespace FclEx.Extensions;

public static partial class InterfaceBaseInvocationExtension
{
    internal readonly record struct InterfaceMethodInfo(Type InstanceType, Type InterfaceType, MethodInfo Method);

    private static (MethodInfo method, Type[] ParaTypes) GetInterfaceMethod(InterfaceMethodInfo info)
    {
        var (instanceType, interfaceType, method) = info;
        var parameters = method.GetParameters();
        var genericArguments = method.GetGenericArguments();
        var paraTypes = parameters.Select(t => t.ParameterType).ToArray();
        var interfaceMethods = instanceType
            .GetInterfaceMap(interfaceType)
            .InterfaceMethods
            .Where(m => IfMatch(method, genericArguments, parameters, m))
            .ToArray();

        var interfaceMethod = interfaceMethods.Length switch
        {
            0 => throw new MissingMethodException($"Can not find method {method.Name} in type {instanceType.LongName()}"),
            > 1 => throw new AmbiguousMatchException($"Found more than one method {method.Name} in type {instanceType.LongName()}"),
            1 when interfaceMethods[0].IsAbstract => throw new InvalidOperationException($"The method {interfaceMethods[0].Name} is abstract"),
            _ => interfaceMethods[0]
        };

        if (method.IsGenericMethod)
            interfaceMethod = interfaceMethod.MakeGenericMethod(method.GetGenericArguments());

        return (interfaceMethod, paraTypes);
    }

    private static bool IfMatch(MethodInfo method, Type[] genericArguments, ParameterInfo[] parameters, MethodInfo interfaceMethod)
    {
        var isSameType = method.DeclaringType == interfaceMethod.DeclaringType;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (isSameType && method.Name != interfaceMethod.Name)
            return false;

        if (!isSameType && !interfaceMethod.Name.EndsWith("." + method.Name))
            return false;

        if (method.IsGenericMethod != interfaceMethod.IsGenericMethod)
            return false;

        if (method.IsGenericMethod)
        {
            if (method.IsGenericMethod && genericArguments.Length != interfaceMethod.GetGenericArguments().Length)
                return false;

            interfaceMethod = interfaceMethod.MakeGenericMethod(genericArguments);
        }

        if (method.ReturnType != interfaceMethod.ReturnType)
            return false;

        var interfaceMethodParameters = interfaceMethod.GetParameters();
        if (parameters.Length != interfaceMethodParameters.Length)
            return false;

        foreach (var (paraType, interfaceParaType) in parameters.Zip(interfaceMethodParameters).Select(m => (m.First.ParameterType, m.Second.ParameterType)))
        {
            if (paraType != interfaceParaType)
                return false;
        }
        return true;
    }

    private static (MethodInfo method, IReadOnlyList<Expression> args) GetMethodAndArguments(Expression exp) => exp switch
    {
        LambdaExpression lambda => GetMethodAndArguments(lambda.Body),
        UnaryExpression unary => GetMethodAndArguments(unary.Operand),
        MethodCallExpression methodCall => (methodCall.Method!, methodCall.Arguments),
        MemberExpression { Member: PropertyInfo prop } => (prop.GetRequiredGetMethod(), []),
        _ => throw new InvalidOperationException("The expression refers to neither a method nor a readable property.")
    };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Base<TInterface>(this TInterface instance, Expression<Action<TInterface>> selector)
    {
        instance.BaseByDynamicMethod(selector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TReturn Base<TInterface, TReturn>(this TInterface instance, Expression<Func<TInterface, TReturn>> selector)
    {
        return instance.BaseByDynamicMethod(selector);
    }
}