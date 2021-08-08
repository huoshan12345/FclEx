using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx
{
    partial class InterfaceBaseInvocationExtension
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
                var (interfaceMethod, _) = GetInterfaceMethod(k.InstanceType, k.InterfaceType, k.Method);
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
                parameterTypes: new[] { interfaceType, typeof(IEnumerable<object>) },
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
