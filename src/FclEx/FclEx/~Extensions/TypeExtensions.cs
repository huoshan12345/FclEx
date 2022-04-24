using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dawn;
using FclEx.Helpers;

namespace FclEx
{
    public static partial class TypeExtensions
    {
        public static object? DefaultValueByExp(this Type type)
        {
            Guard.Argument(type, nameof(type)).NotNull();

            // We want an Func<object> which returns the default.
            // Create that expression here.
            var e = Expression.Lambda<Func<object?>>(
                // Have to convert to object.
                Expression.Convert(
                    // The default value, always get what the *code* tells us.
                    Expression.Default(type), typeof(object)
                )
            );

            // Compile and return the value.
            return e.Compile()();
        }

        public static object CreateObject(this Type type, params object?[] args)
        {
            Guard.Argument(type, nameof(type)).NotNull();

            if (args.IsNullOrEmpty())
                return Activator.CreateInstance(type)!;

            var argsType = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var ctor = type.GetConstructors().FirstOrDefault(m => m.ArgumentListMatches(argsType));
            if (ctor != null)
            {
                var paras = ctor.GetParameters();
                if (paras.Length > args.Length)
                {
                    args = args.Concat(paras.Skip(args.Length).Select(m => m.RawDefaultValue)).ToArray();
                }
                return ctor.Invoke(args);
            }

            throw new MissingMethodException();
        }

        public static MethodInfo GetMethod(this Type type, string methodName, int pParametersCount = 0, int pGenericArgumentsCount = 0)
        {
            Guard.Argument(type, nameof(type)).NotNull();

            return type.GetMethods()
                    .Where(m => m.Name == methodName)
                    .Select(m => new
                    {
                        Method = m,
                        Params = m.GetParameters(),
                        Args = m.GetGenericArguments()
                    })
                    .Where(x => x.Params.Length == pParametersCount
                                && x.Args.Length == pGenericArgumentsCount
                    ).Select(x => x.Method)
                    .First();
        }

        public static bool SequenceAssignableFrom(this IEnumerable<Type> first, IEnumerable<Type> second)
        {
            var comparer = EqualityComparer<Type>.Default;
            using (var e1 = first.GetEnumerator())
            {
                using (var e2 = second.GetEnumerator())
                {
                    while (e1.MoveNext())
                    {
                        if (!e2.MoveNext()) return false;
                        else if (!(comparer.Equals(e1.Current, e2.Current) || e1.Current.IsAssignableFrom(e2.Current)))
                            return false;
                    }
                    if (e2.MoveNext())
                        return false;
                }
            }
            return true;
        }

        public static bool IsInheritedFromGenericType(this Type type, Type genericType)
        {
            return GetGenericInterface(type, genericType) != null;
        }

        public static Type? GetGenericInterface(this Type type, Type genericType)
        {
            return type.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == genericType);
        }

        public static bool IsSubclassOfRawGeneric(this Type? toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static IEnumerable<DataMemberInfo> GetDataMembers(this Type type) => ReflectionHelper.GetDataMembers(type).SelectMany(m => m.Value);

        public static DataMemberInfo? GetDataMember(this Type type, string name) => ReflectionHelper.GetDataMembers(type).GetOrEmptyArr(name).LastOrDefault();

        public static T? GetMemberValue<T>(this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Static
                                       | BindingFlags.Public | BindingFlags.NonPublic
                                       | BindingFlags.GetField | BindingFlags.GetProperty;
            return type.InvokeMember(name, flags, null, null, null).CastTo<T>();
        }

        public static T? GetMemberValue<T>(this Type type, string name, object? obj)
        {
            const BindingFlags flags = BindingFlags.Instance
                                       | BindingFlags.Public | BindingFlags.NonPublic
                                       | BindingFlags.GetField | BindingFlags.GetProperty;
            return type.InvokeMember(name, flags, null, obj, null).CastTo<T>();
        }

        public static bool IsDynamic(this Type type)
        {
            return type.IsDefined<DynamicAttribute>(true);
        }
    }
}
