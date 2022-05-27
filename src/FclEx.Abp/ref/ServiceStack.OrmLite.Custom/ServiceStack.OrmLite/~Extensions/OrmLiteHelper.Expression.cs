using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.Helpers;

namespace ServiceStack.OrmLite
{
    partial class OrmLiteHelper
    {
        internal static Expression<Func<T, bool>> BuildFilterById<T>(PropertyInfo idProp, object id)
        {
            return BuildFilter<T>(idProp, id, Expression.Equal);
        }

        public static Expression<Func<T, bool>> BuildFilterById<T>(T obj)
        {
            var idField = GetIdField<T>();
            var idProp = idField.PropertyInfo;
            var id = idProp.GetValue(obj);
            return BuildFilterById<T>(idProp, id);
        }

        public static Expression<Func<T, bool>> BuildFilterById<T>(object id)
        {
            var idField = GetIdField<T>();
            var idProp = idField.PropertyInfo;
            return BuildFilterById<T>(idProp, id);
        }

        internal static Expression<Func<T, bool>> BuildFilter<T>(MemberInfo memberInfo, object value, Func<Expression, Expression, BinaryExpression> builder)
        {
            var type = typeof(T);
            var parameter = Expression.Parameter(type, type.Name);
            var constant = Expression.Constant(value);
            var (member, memberType) = memberInfo switch
            {
                PropertyInfo property => (Expression.Property(parameter, property), property.PropertyType),
                FieldInfo field => (Expression.Field(parameter, field), field.FieldType),
                _ => throw new ArgumentException($"MemberInfo '{memberInfo.Name}' refers to neither a field nor a property.")
            };
            var convert = Expression.Convert(constant, memberType);
            var e = builder(member, convert);
            var predicate = Expression.Lambda<Func<T, bool>>(e, parameter);
            return predicate;
        }

        public static Expression<Func<T, bool>> BuildFilter<T>(Expression<Func<T, object>> selector, object value, Func<Expression, Expression, BinaryExpression> builder)
        {
            var member = ExpressionHelper.GetDataMember(selector);
            return BuildFilter<T>(member, value, builder);
        }

        public static Expression<Func<T, bool>> BuildFilterOfEqual<T>(Expression<Func<T, object>> selector, object value)
        {
            return BuildFilter(selector, value, Expression.Equal);
        }

        public static Expression<Func<T, bool>> BuildFilter<T>(Expression<Func<T, object>> selector, Func<MemberInfo, BinaryExpression> builder)
        {
            var member = ExpressionHelper.GetDataMember(selector);
            var equal = builder(member);
            var predicate = Expression.Lambda<Func<T, bool>>(equal, selector.Parameters);
            return predicate;
        }
    }
}
