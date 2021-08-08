using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FclEx
{
    public static partial class InterfaceBaseInvocationExtension
    {
        private static readonly ConcurrentDictionary<(Type, Type, MethodInfo), (IntPtr, MethodInfo)> MethodMap = new();

        private static readonly Dictionary<int, Type> ActionTypes = typeof(Action).Assembly
            .GetExportedTypes()
            .Where(m => m.SimpleName() == nameof(Action))
            .ToDictionary(m => m.GetTypeInfo().GenericTypeParameters.Length);

        private static readonly Dictionary<int, Type> FuncTypes = typeof(Func<>).Assembly
            .GetExportedTypes()
            .Where(m => m.SimpleName() == nameof(Func<int>))
            .ToDictionary(m => m.GetTypeInfo().GenericTypeParameters.Length);

        public static void Base<TInterface>(this TInterface instance, Expression<Action<TInterface>> selector)
        {
            var (invoke, func, args) = GetInterfaceFunc(instance, selector);
            invoke.Invoke(func, args);
        }

        public static TReturn Base<TInterface, TReturn>(this TInterface instance, Expression<Func<TInterface, TReturn>> selector)
        {
            var (invoke, func, args) = GetInterfaceFunc(instance, selector);
            return invoke.Invoke(func, args).CastTo<TReturn>()!;
        }

        private static (MethodInfo method, Type[] ParaTypes) GetInterfaceMethod(Type instanceType, Type interfaceType, MethodInfo method)
        {
            var paras = method.GetParameters();
            var paraTypes = paras.Select(t => t.ParameterType).ToArray();
            var map = instanceType.GetInterfaceMap(interfaceType);
            var interfaceMethods = map.InterfaceMethods
                .Where(m => InterfaceMethodNameMatch(interfaceType, method, m) && m.GetParameters().Select(x => x.ParameterType).SequenceEqual(paraTypes))
                .ToArray();

            if (interfaceMethods.Length == 0)
                throw new MissingMethodException($"Can not find method {method.Name} in type {instanceType.LongName()}");

            if (interfaceMethods.Length > 1)
                throw new AmbiguousMatchException($"Found more than one method {method.Name} in type {instanceType.LongName()}");

            var interfaceMethod = interfaceMethods[0];

            if (interfaceMethod.IsAbstract)
                throw new InvalidOperationException($"The method {interfaceMethod.Name} is abstract");

            if (method.IsGenericMethod)
                interfaceMethod = interfaceMethod.MakeGenericMethod(method.GetGenericArguments());

            return (interfaceMethod, paraTypes);
        }

        private static (IntPtr pointer, MethodInfo invoke) GetInterfaceMethodPointer(Type instanceType, Type interfaceType, MethodInfo method)
        {
            var (interfaceMethod, paraTypes) = GetInterfaceMethod(instanceType, interfaceType, method);

            var ifReturnVoid = method.ReturnType == typeof(void);
            var actionType = GetDelegateType(ifReturnVoid, paraTypes.Length);

            var types = ifReturnVoid
                ? paraTypes
                : paraTypes.Append(method.ReturnType).ToArray();

            var genericType = actionType.MakeGenericType(types);
            var functionPointer = interfaceMethod.MethodHandle.GetFunctionPointer();
            return (functionPointer, genericType.GetMethod(nameof(Action.Invoke))!);
        }

        private static (MethodInfo invoke, object func, object?[] args) GetInterfaceFunc<TInterface>(this TInterface instance, LambdaExpression selector)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var (method, args) = GetMethodArgs(selector);
            var interfaceType = typeof(TInterface);
            var (pointer, invoke) = MethodMap.GetOrAdd((instance.GetType(), interfaceType, method), m => GetInterfaceMethodPointer(m.Item1, m.Item2, m.Item3));
            var func = Activator.CreateInstance(invoke.DeclaringType!, instance, pointer);
            return (invoke, func!, args);
        }

        private static bool InterfaceMethodNameMatch(Type interfaceType, MethodInfo method, MethodInfo interfaceMethod)
        {
            var iName = interfaceMethod.Name;
            var isNewMethod = interfaceType == method.DeclaringType; // method with new keyword
            return isNewMethod && method.Name == iName
                   || !isNewMethod && iName.Contains(method.Name) && iName.Contains(method.DeclaringType!.SimpleName());
        }

        private static Type GetDelegateType(bool ifReturnVoid, int len)
        {
            var (key, dic, t) = ifReturnVoid
                ? (len, ActionTypes, nameof(Action))
                : (len + 1, FuncTypes, nameof(Func<object>));
            return dic.Get(key) ?? throw new NotSupportedException($"Cannot find {t} type with {key} arguments");
        }

        private static (MethodInfo method, object?[] args) GetMethodArgs(Expression exp) => exp switch
        {
            LambdaExpression lambda => GetMethodArgs(lambda.Body),
            UnaryExpression unary => GetMethodArgs(unary.Operand),
            MethodCallExpression methodCall => (methodCall.Method!, methodCall.Arguments.GetArgumentValues()),
            MemberExpression { Member: PropertyInfo prop } => (prop.GetRequiredGetMethod(), Array.Empty<object?>()),
            _ => throw new InvalidOperationException("The expression refers to neither a method nor a readable property.")
        };
    }
}
