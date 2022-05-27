using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using FclEx.Helpers;

namespace ServiceStack.OrmLite
{
    using FclEx;
    using FclEx.Utils;

    public static partial class SqlExpExtensions
    {
        public static SqlExpression<T> NotEqual<T, TEntity>(this SqlExpression<T> exp, Expression<Func<TEntity, object>> selector, object value)
        {
            var where = OrmLiteHelper.BuildFilter(selector, value, Expression.NotEqual);
            return exp.Where(where);
        }

        public static SqlExpression<T> NotEqual<T>(this SqlExpression<T> exp, Expression<Func<T, object>> selector, object value)
        {
            return exp.NotEqual<T, T>(selector, value);
        }

        public static SqlExpression<T> NotNull<T, TEntity>(this SqlExpression<T> exp, Expression<Func<TEntity, object>> selector)
        {
            var expOfNotEqual = Expression.NotEqual(selector.Body, Expression.Constant(null));
            var predicate = Expression.Lambda<Func<TEntity, bool>>(expOfNotEqual, selector.Parameters);
            return exp.Where(predicate);
        }

        public static SqlExpression<T> NotNull<T>(this SqlExpression<T> exp, Expression<Func<T, object>> selector)
        {
            return exp.NotNull<T, T>(selector);
        }

        public static SqlExpression<T> IsValid<T, TEntity>(this SqlExpression<T> exp, Expression<Func<TEntity, object>> selector)
        {
            var member = ExpressionHelper.GetDataMember(selector);
            var type = member.GetDataMemberType();
            var (@null, @default) = GetDefaultValue(type);
            if (@null == null)
            {
                return exp.NotNullNorValue(selector, @default);
            }
            else
            {
                return exp.NotEqual(selector, @default);
            }
        }

        public static SqlExpression<T> IsValid<T>(this SqlExpression<T> exp, Expression<Func<T, object>> selector)
        {
            return exp.IsValid<T, T>(selector);
        }

        public static SqlExpression<T> NotNullNorValue<T, TEntity>(this SqlExpression<T> exp, Expression<Func<TEntity, object>> selector, object value)
        {
            exp = exp.NotNull(selector);
            if (value != null)
            {
                exp = exp.NotEqual(selector, value);
            }
            return exp;
        }

        public static SqlExpression<T> NotNullNorValue<T>(this SqlExpression<T> exp, Expression<Func<T, object>> selector, object value)
        {
            return exp.NotNullNorValue<T, T>(selector, value);
        }
    }
}
