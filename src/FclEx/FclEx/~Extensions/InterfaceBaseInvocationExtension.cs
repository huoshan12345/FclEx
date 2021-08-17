using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using FclEx.Helpers;

namespace FclEx
{
    public static partial class InterfaceBaseInvocationExtension
    {
        internal readonly struct InterfaceMethodInfo
        {
            public bool Equals(InterfaceMethodInfo other)
            {
                return InstanceType == other.InstanceType
                       && InterfaceType == other.InterfaceType
                       && Method.Equals(other.Method);
            }

            public override bool Equals(object? obj)
            {
                return obj is InterfaceMethodInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(InstanceType, InterfaceType, Method);
            }

            public readonly Type InstanceType;
            public readonly Type InterfaceType;
            public readonly MethodInfo Method;

            public InterfaceMethodInfo(Type instanceType, Type interfaceType, MethodInfo method)
            {
                InstanceType = instanceType;
                InterfaceType = interfaceType;
                Method = method;
            }

            public void Deconstruct(out Type instanceType, out Type interfaceType, out MethodInfo method)
            {
                instanceType = InstanceType;
                interfaceType = InterfaceType;
                method = Method;
            }
        }

        private static (MethodInfo method, Type[] ParaTypes) GetInterfaceMethod(InterfaceMethodInfo info)
        {
            var (instanceType, interfaceType, method) = info;
            var paras = method.GetParameters();
            var paraTypes = paras.Select(t => t.ParameterType).ToArray();
            var interfaceMethods = instanceType
                .GetInterfaceMap(interfaceType)
                .InterfaceMethods
                .Where(m => InterfaceMethodNameMatch(interfaceType, method, m) && m.GetParameters().Select(x => x.ParameterType).SequenceEqual(paraTypes))
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

        private static bool InterfaceMethodNameMatch(Type interfaceType, MethodInfo method, MethodInfo interfaceMethod)
        {
            var iName = interfaceMethod.Name;
            var isSameType = interfaceType == method.DeclaringType;
            return isSameType && method.Name == iName
                   || !isSameType && iName.EndsWith("." + method.Name);
        }

        private static (MethodInfo method, IReadOnlyList<Expression> args) GetMethodAndArguments(Expression exp) => exp switch
        {
            LambdaExpression lambda => GetMethodAndArguments(lambda.Body),
            UnaryExpression unary => GetMethodAndArguments(unary.Operand),
            MethodCallExpression methodCall => (methodCall.Method!, methodCall.Arguments),
            MemberExpression { Member: PropertyInfo prop } => (prop.GetRequiredGetMethod(), Array.Empty<Expression>()),
            _ => throw new InvalidOperationException("The expression refers to neither a method nor a readable property.")
        };
    }
}
