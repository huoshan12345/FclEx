using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.Helpers;

namespace FclEx.Extensions;

partial class InterfaceBaseInvocationExtension
{
    private static readonly ConcurrentDictionary<InterfaceMethodInfo, (IntPtr, MethodInfo)> MethodMap = new();

    internal static void BaseByDelegate<TInterface>(this TInterface instance, Expression<Action<TInterface>> selector)
    {
        var (invoke, invoker, args) = GetInterfaceFunc(instance, selector);
        invoke.Invoke(invoker, args);
    }

    internal static TReturn BaseByDelegate<TInterface, TReturn>(this TInterface instance, Expression<Func<TInterface, TReturn>> selector)
    {
        var (invoke, invoker, args) = GetInterfaceFunc(instance, selector);
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
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        var (method, args) = GetMethodAndArguments(selector);
        var evaluatedArguments = args.GetArgumentValues().ToArray();
        var interfaceType = typeof(TInterface);
        var (pointer, invoke) = MethodMap.GetOrAdd(new(instance.GetType(), interfaceType, method), m => GetInterfaceMethodDelegate(m));
        var invoker = Activator.CreateInstance(invoke.DeclaringType!, instance, pointer);
        return (invoke, invoker!, evaluatedArguments);
    }

    public static void Base<TInterface>(this TInterface instance, Expression<Action<TInterface>> selector)
    {
        instance.BaseByDynamicMethod(selector);
    }

    public static TReturn Base<TInterface, TReturn>(this TInterface instance, Expression<Func<TInterface, TReturn>> selector)
    {
        return instance.BaseByDynamicMethod(selector);
    }
}