using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using FclEx.Extensions;

namespace ServiceStack.OrmLite
{
    using FclEx;
    using FclEx.Utils;

    public static partial class SqlExpExtensions
    {
        private static readonly ConcurrentDictionary<Type, (object, object)> CacheOfDefaultValue = new ConcurrentDictionary<Type, (object, object)>();

        private static (object Null, object Default) GetDefaultValue(Type type)
        {
            return CacheOfDefaultValue.GetOrAdd(type, t =>
            {
                if (type == typeof(string))
                {
                    return (null, string.Empty);
                }
                else if (type.IsValueType)
                {
                    var value = type.UnwarpNullable().DefaultValue();
                    return (type.IsNullable() ? null : value, value);
                }
                else
                {
                    return (null, null);
                }
            });
        }

        public static SqlExpression<T> WhereById<T>(this SqlExpression<T> exp, object id)
        {
            var where = OrmLiteHelper.BuildFilterById<T>(id);
            return exp.Where(where);
        }

        public static SqlExpression<T> WhereById<T>(this SqlExpression<T> exp, T item)
        {
            var where = OrmLiteHelper.BuildFilterById<T>(item);
            return exp.Where(where);
        }

        public static SqlExpression<T> Equal<T, TEntity>(this SqlExpression<T> exp, Expression<Func<TEntity, object>> selector, object value)
        {
            var where = OrmLiteHelper.BuildFilterOfEqual(selector, value);
            return exp.Where(where);
        }

        public static SqlExpression<T> Equal<T>(this SqlExpression<T> exp, Expression<Func<T, object>> selector, object value)
        {
            return exp.Equal<T, T>(selector, value);
        }

        public static SqlExpression<T> EqualIf<T, TEntity>(this SqlExpression<T> exp, bool condition, Expression<Func<TEntity, object>> selector, object value)
        {
            return condition ? exp.Equal(selector, value) : exp;
        }

        public static SqlExpression<T> EqualIf<T>(this SqlExpression<T> exp, bool condition, Expression<Func<T, object>> selector, object value)
        {
            return exp.EqualIf<T, T>(condition, selector, value);
        }

        public static SqlExpression<T> EqualIfValid<T, TEntity>(this SqlExpression<T> exp, Expression<Func<TEntity, object>> selector, object value)
        {
            if (value == null)
                return exp;

            var type = value.GetType();
            var (_, @default) = GetDefaultValue(type);
            if (Equals(value, @default))
                return exp;

            return exp.Equal(selector, value);
        }

        public static SqlExpression<T> EqualIfValid<T>(this SqlExpression<T> exp, Expression<Func<T, object>> selector, object value)
        {
            return exp.EqualIfValid<T, T>(selector, value);
        }

        public static SqlExpression<T> EqualIfNotNull<T, TEntity>(this SqlExpression<T> exp, Expression<Func<TEntity, object>> selector, object value)
        {
            return exp.EqualIf(value != null, selector, value);
        }

        public static SqlExpression<T> EqualIfNotNull<T>(this SqlExpression<T> exp, Expression<Func<T, object>> selector, object value)
        {
            return exp.EqualIfNotNull<T, T>(selector, value);
        }
    }
}
