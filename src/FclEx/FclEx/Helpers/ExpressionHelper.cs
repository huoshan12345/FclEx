using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FclEx.Helpers
{
    public static class ExpressionHelper
    {
        public static PropertyInfo GetProp<TSource, TMember>(Expression<Func<TSource, TMember>> selector)
        {
            var member = GetMember(selector);
            if (member is PropertyInfo info) return info;
            throw new ArgumentException($"Expression '{selector}' does not refer to a property.");
        }

        public static FieldInfo GetField<TSource, TMember>(Expression<Func<TSource, TMember>> selector)
        {
            var member = GetMember(selector);
            if (member is FieldInfo info) return info;
            throw new ArgumentException($"Expression '{selector}' does not refer to a field.");
        }

        public static MethodInfo GetMethod<TSource, TMember>(Expression<Func<TSource, TMember>> selector)
        {
            var member = GetMember(selector);
            if (member is MethodInfo info) return info;
            throw new ArgumentException($"Expression '{selector}' does not refer to a method.");
        }

        public static MemberInfo GetMember<T, TMember>(Expression<Func<T, TMember>> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            if (!(selector.Body is MemberExpression member))
                throw new ArgumentException($"Expression '{selector}' refers to a method, not a property.");

            var reflectedType = member.Member.ReflectedType;

            //If the MemberInfo object is a global member (that is, if it was obtained from the Module.GetMethods method,
            //which returns global methods on a module), the returned DeclaringType will be null.
            if (reflectedType == null)
                throw new ArgumentException($"Expression '{selector}' does not refer to a property of a class.");

            var type = typeof(T);
            if (type != reflectedType && !type.IsSubclassOf(reflectedType))
                throw new ArgumentException($"Expression '{selector}' refers to a property that is not from type {type}.");

            return member.Member;
        }

        public static Action<T, TMember> GetSetter<T, TMember>(Expression<Func<T, TMember>> selector)
        {
            var member = GetMember(selector);
            switch (member)
            {
                case PropertyInfo propInfo:
                {
                    var setter = (Action<T, TMember>)((o, v) => propInfo.SetValue(o, v));
                    return setter;
                }
                case FieldInfo fieldInfo:
                {
                    var setter = (Action<T, TMember>)((o, v) => fieldInfo.SetValue(o, v));
                    return setter;
                }
                default:
                    throw new ArgumentException($"Expression '{selector}' refers to neither a field nor a property.");
            }
        }

        public static Expression<Func<T, object>> ErasureType<T, TProp>(Expression<Func<T, TProp>> selector)
        {
            var type = typeof(TProp);
            if (type != typeof(object))
            {
                var e = Expression.Convert(selector.Body, typeof(object));
                return Expression.Lambda<Func<T, object>>(e, selector.Parameters);
            }
            else
            {
                return Expression.Lambda<Func<T, object>>(selector.Body, selector.Parameters);
            }
        }
    }
}
