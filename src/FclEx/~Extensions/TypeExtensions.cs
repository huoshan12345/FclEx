using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dawn;
using FclEx.Utils;

namespace FclEx
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return type.IsValueType
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type UnwarpNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static MethodInfo GetMethod(this Type type, string methodName, int pParametersCount = 0, int pGenericArgumentsCount = 0)
        {
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

        public static object GetDefault(this Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
            else
                return null;
        }

        public static object GetDefaultByExp(this Type type)
        {
            // Validate parameters.
            if (type == null) throw new ArgumentNullException(nameof(type));

            // We want an Func<object> which returns the default.
            // Create that expression here.
            var e = Expression.Lambda<Func<object>>(
                // Have to convert to object.
                Expression.Convert(
                    // The default value, always get what the *code* tells us.
                    Expression.Default(type), typeof(object)
                )
            );

            // Compile and return the value.
            return e.Compile()();
        }

        public static object CreateObject(this Type type, params object[] args)
        {
            Guard.Argument(type, nameof(type)).NotNull();

            if (args.IsNullOrEmpty()) return Activator.CreateInstance(type);

            var argsType = args.Select(a => a.GetType()).ToArray();
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

        public static bool IsInheritedFromGenericType(this Type type, Type genericType)
        {
            return GetGenericInterface(type, genericType) != null;
        }

        public static Type GetGenericInterface(this Type type, Type genericType)
        {
            return type.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == genericType);
        }

        public static Type GetAnyElementType(this Type type)
        {
            // Type is Array
            // short-circuit if you expect lots of arrays 
            if (type.IsArray)
                return type.GetElementType();

            // type is IEnumerable<T>;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GenericTypeArguments[0];

            // type implements/extends IEnumerable<T>;
            var enumType = type.GetGenericInterface(typeof(IEnumerable<>));
            if (enumType != null)
                return enumType.GenericTypeArguments[0];

            return null;
        }

        /// <summary>
        /// Get type name without any generics info
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string SimpleName(this Type type)
        {
            if (!type.IsGenericType) return type.Name;
            var name = type.Name;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        /// <summary>
        /// Get name of type with generic parameters without namespace.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ShortName(this Type type)
        {
            if (!type.IsGenericType) return type.Name;
            var typeName = type.SimpleName();
            var paraName = string.Join(", ", type.GenericTypeArguments.Select(m => m.ShortName()));
            return typeName + "<" + paraName + ">";
        }

        /// <summary>
        /// Get name of type with generic parameters with namespace.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string LongName(this Type type)
        {
            return type.Namespace + type.ShortName();
        }

        public static bool IsInteger(this Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(long)
                   || type == typeof(ulong)
                   || type == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(byte)
                   || type == typeof(sbyte);
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
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
    }
}
