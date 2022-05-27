using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ServiceStack.OrmLite
{
    using FclEx;
    using FclEx.Utils;

    public static partial class SqlExpExtensions
    {
        public static SqlExpression<T> Between<T, TTarget>(this SqlExpression<T> exp,
            Expression<Func<T, TTarget>> selector, Range<TTarget> range)
            where TTarget : struct
        {
            if (range.Max.Value.HasValue)
            {
                exp = exp.LessThan(selector, range.Max.Value.Value, range.Max.IncludeEqual);
            }
            if (range.Min.Value.HasValue)
            {
                exp = exp.GreaterThan(selector, range.Min.Value.Value, range.Min.IncludeEqual);
            }
            return exp;
        }

        public static SqlExpression<T> Between<T, TTarget>(this SqlExpression<T> exp,
            Expression<Func<T, TTarget?>> selector, TTarget? min, TTarget? max)
            where TTarget : struct
        {
            if (max.HasValue)
            {
                exp = LessThan(exp, selector, max);
            }
            if (min.HasValue)
            {
                exp = GreaterThan(exp, selector, min);
            }
            return exp;
        }

        public static SqlExpression<T> LessThan<T, TTarget, TValue>(this SqlExpression<T> exp,
            Expression<Func<T, TTarget>> selector, TValue max, bool includeEqual = true)
        {
            var expression = includeEqual
                ? Expression.LessThanOrEqual(selector.Body, Expression.Constant(max, typeof(TValue)))
                : Expression.LessThan(selector.Body, Expression.Constant(max, typeof(TValue)));
            var lambda = Expression.Lambda<Func<T, bool>>(expression, selector.Parameters);
            return exp.Where(lambda);
        }

        public static SqlExpression<T> LessThanIfNotNull<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp>> selector, TProp? value, bool includeEqual = true) where TProp : struct
        {
            if (value != null)
            {
                var constant = Expression.Constant(value, value.GetType());
                var equal = includeEqual
                    ? Expression.LessThanOrEqual(selector.Body, constant)
                    : Expression.LessThan(selector.Body, constant);
                var where = Expression.Lambda<Func<T, bool>>(equal, selector.Parameters);
                exp.Where(where);
            }
            return exp;
        }

        public static SqlExpression<T> GreaterThan<T, TTarget, TValue>(this SqlExpression<T> exp,
            Expression<Func<T, TTarget>> selector, TValue min, bool includeEqual = true)
        {
            var expression = includeEqual
                ? Expression.GreaterThanOrEqual(selector.Body, Expression.Constant(min, typeof(TValue)))
                : Expression.GreaterThan(selector.Body, Expression.Constant(min, typeof(TValue)));
            var lambda = Expression.Lambda<Func<T, bool>>(expression, selector.Parameters);
            return exp.Where(lambda);
        }

        public static SqlExpression<T> GreaterThanIfNotNull<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp>> selector, TProp? value, bool includeEqual = true)
            where TProp : struct
        {
            if (value != null)
            {
                var constant = Expression.Constant(value, value.GetType());
                var equal = includeEqual
                    ? Expression.GreaterThanOrEqual(selector.Body, constant)
                    : Expression.GreaterThan(selector.Body, constant);
                var where = Expression.Lambda<Func<T, bool>>(equal, selector.Parameters);
                exp.Where(where);
            }
            return exp;
        }

        public static SqlExpression<T> NonNegative<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp>> selector)
        {
            var constant = Expression.Constant(0, typeof(TProp));
            var equal = Expression.GreaterThanOrEqual(selector.Body, constant);
            var where = Expression.Lambda<Func<T, bool>>(equal, selector.Parameters);
            exp.Where(where);
            return exp;
        }
    }
}
