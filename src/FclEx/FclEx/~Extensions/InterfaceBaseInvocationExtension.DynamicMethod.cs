using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dawn;

namespace FclEx
{
    partial class InterfaceBaseInvocationExtension
    {
        public static void BaseByDynamicMethod<TInterface>(this TInterface instance, Expression<Action<TInterface>> selector)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var (method, args) = GetMethodAndArguments(selector);
            var interfaceType = typeof(TInterface);
            var (interfaceMethod, _) = GetInterfaceMethod(instance!.GetType(), interfaceType, method);
            var dynamicMethod = GetDynamicMethod(interfaceType, interfaceMethod, args);
            var caller = dynamicMethod.CreateDelegate<Action<TInterface, object?[]>>();
            var evaluatedArguments = args.GetArgumentValues();
            caller(instance, evaluatedArguments);
        }

        public static TReturn BaseByDynamicMethod<TInterface, TReturn>(this TInterface instance, Expression<Func<TInterface, TReturn>> selector)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var (method, args) = GetMethodAndArguments(selector);
            var interfaceType = typeof(TInterface);
            var (interfaceMethod, _) = GetInterfaceMethod(instance!.GetType(), interfaceType, method);
            var dynamicMethod = GetDynamicMethod(interfaceType, interfaceMethod, args);
            var caller = dynamicMethod.CreateDelegate<Func<TInterface, object?[], TReturn>>();
            var evaluatedArguments = args.GetArgumentValues();
            return caller(instance, evaluatedArguments);
        }

        private static DynamicMethod GetDynamicMethod(Type interfaceType, MethodInfo method, IReadOnlyList<Expression> arguments)
        {
            var dynamicMethod = new DynamicMethod(
                name: "__IL_" + method.GetFullName(),
                returnType: method.ReturnType,
                parameterTypes: new[] { interfaceType, typeof(object[]) },
                owner: typeof(object),
                skipVisibility: true);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < arguments.Count; ++i)
            {
                var argumentType = arguments[i].Type;
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem, typeof(object));
                if (argumentType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, argumentType);
                }
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
