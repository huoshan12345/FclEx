using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace FclEx.Extensions;

partial class InterfaceBaseInvocationExtension
{
    private static readonly ConcurrentDictionary<InterfaceMethodInfo, Delegate> _delegates = new();

    public static void BaseByDynamicMethod<TInterface>(this TInterface instance, Expression<Action<TInterface>> selector)
    {
        var (func, args) = GetDynamicMethod<TInterface, Unit>(instance, selector);
        ((Action<TInterface, object?[]>)func)(instance, args);
    }

    public static TReturn BaseByDynamicMethod<TInterface, TReturn>(this TInterface instance, Expression<Func<TInterface, TReturn>> selector)
    {
        var (func, args) = GetDynamicMethod<TInterface, TReturn>(instance, selector);
        return ((Func<TInterface, object?[], TReturn>)func)(instance, args);
    }

    private static (Delegate, object?[]) GetDynamicMethod<TInterface, TReturn>(TInterface instance, LambdaExpression selector)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        var (method, args) = GetMethodAndArguments(selector);
        var evaluatedArguments = args.GetArgumentValues().ToArray();
        var func = _delegates.GetOrAdd(new(instance.GetType(), typeof(TInterface), method), k =>
        {
            var (interfaceMethod, _) = GetInterfaceMethod(k);
            var dynamicMethod = GetDynamicMethod(k.InterfaceType, interfaceMethod, args.Select(m => m.Type));
            var ifReturnVoid = method.ReturnType == typeof(void);
            return ifReturnVoid
                ? dynamicMethod.CreateDelegate<Action<TInterface, object[]>>()
                : dynamicMethod.CreateDelegate<Func<TInterface, object[], TReturn>>();
        });
        return (func, evaluatedArguments);
    }

    private static DynamicMethod GetDynamicMethod(Type interfaceType, MethodInfo method, IEnumerable<Type> argumentTypes)
    {
        var dynamicMethod = new DynamicMethod(
            name: "__IL_" + method.GetFullName(),
            returnType: method.ReturnType,
            parameterTypes: new[] { interfaceType, typeof(object[]) },
            owner: typeof(object),
            skipVisibility: true);

        var il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);

        var i = 0;
        foreach (var argumentType in argumentTypes)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem, typeof(object));
            if (argumentType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, argumentType);
            }
            ++i;
        }
        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ret);
        return dynamicMethod;
    }
}