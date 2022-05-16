using System;
using System.Linq.Expressions;
using System.Reflection;
using FclEx;

namespace FclEx.Helpers
{
    public static class ExpressionHelper
    {
        public static PropertyInfo GetProperty<TSource, TMember>(Expression<Func<TSource, TMember>> selector)
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

        public static MethodInfo GetMethod<TSource>(Expression<Action<TSource>> selector)
        {
            var member = GetMember(selector);
            if (member is MethodInfo info) return info;
            throw new ArgumentException($"Expression '{selector}' does not refer to a method.");
        }

        public static MethodInfo GetMethod<TSource, TMember>(Expression<Func<TSource, TMember>> selector)
        {
            var member = GetMember(selector);
            if (member is MethodInfo info) return info;
            throw new ArgumentException($"Expression '{selector}' does not refer to a method.");
        }

        public static MemberInfo GetMember(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return expression switch
            {
                MethodCallExpression methodCall => methodCall.Method,
                LambdaExpression lambda => GetMember(lambda.Body),
                UnaryExpression unary => GetMember(unary.Operand),
                MemberExpression member => member.Member,
                _ => throw new ArgumentException($"Expression '{expression}' does not refer to a member.")
            };
        }

        public static MemberInfo GetMember(Expression expression, Type type)
        {
            var member = GetMember(expression);

            var reflectedType = member.ReflectedType;

            // If the MemberInfo object is a global member (that is, if it was obtained from the Module.GetMethods method,
            // which returns global methods on a module), the returned DeclaringType will be null.
            if (reflectedType == null)
                throw new ArgumentException($"Expression '{expression}' does not refer to a member of a class.");

            if (type != reflectedType && !type.IsSubclassOf(reflectedType))
                throw new ArgumentException($"Expression '{expression}' refers to a member that is not from type {type.LongName()}.");

            return member;
        }

        public static MemberInfo GetMember<T>(Expression<Func<T, object>> selector)
        {
            return GetMember(selector, typeof(T));
        }

        public static MemberInfo GetMember<T>(Expression<Action<T>> selector)
        {
            return GetMember(selector, typeof(T));
        }

        public static MemberInfo GetDataMember<T, TMember>(Expression<Func<T, TMember>> selector)
        {
            var member = GetMember(selector);
            return member switch
            {
                PropertyInfo prop => prop,
                FieldInfo field => field,
                _ => throw new ArgumentException($"Expression '{selector}' refers to neither a field nor a property.")
            };
        }

        public static DataMemberInfo GetDataMemberInfo<T, TMember>(Expression<Func<T, TMember>> selector)
        {
            var member = GetMember(selector);
            return member.ToDataMemberInfo();
        }

        public static Action<T, TMember> GetSetter<T, TMember>(Expression<Func<T, TMember>> selector)
        {
            var member = GetDataMemberInfo(selector);
            return (o, v) => member.SetValue(o, v);
        }

        public static Func<T, TMember?> GetGetter<T, TMember>(Expression<Func<T, TMember?>> selector)
        {
            var member = GetDataMemberInfo(selector);
            return o => member.GetValue(o).CastTo<TMember>();
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

        public static Type GetDataMemberType(this MemberInfo member)
        {
            return member switch
            {
                PropertyInfo propInfo => propInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                DataMemberInfo dataMemberInfo => dataMemberInfo.DataMemberType,
                _ => throw new ArgumentException($"MemberInfo '{member.Name}' refers to neither a field nor a property.")
            };
        }
    }
}
