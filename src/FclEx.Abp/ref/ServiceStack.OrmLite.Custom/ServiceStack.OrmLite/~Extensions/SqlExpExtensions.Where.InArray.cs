using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static ServiceStack.OrmLite.CommonMethods;

namespace ServiceStack.OrmLite
{
    public static partial class SqlExpExtensions
    {
        public static SqlExpression<T> InArray<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp>> selector, IEnumerable<TProp> ids)
        {
            var method = ContainsOfEnumerable.MakeGenericMethod(typeof(TProp));
            var argOfArr = Expression.Constant(ids, ids.GetType());
            var callExp = Expression.Call(null, method, argOfArr, selector.Body);
            var where = Expression.Lambda<Func<T, bool>>(callExp, selector.Parameters);
            exp.Where(where);
            return exp;
        }

        public static SqlExpression<T> InArrayIfAny<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp>> selector, ICollection<TProp> ids)
        {
            return ids.Any() ? exp.InArray(selector, ids) : exp;
        }

        public static SqlExpression<T> NotInArray<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp>> selector, IEnumerable<TProp> ids)
        {
            var argOfArr = Expression.Constant(ids, ids.GetType());
            var method = ContainsOfEnumerable.MakeGenericMethod(typeof(TProp));
            var callExp = Expression.Call(null, method, argOfArr, selector.Body);
            var notExp = Expression.Not(callExp);
            var where = Expression.Lambda<Func<T, bool>>(notExp, selector.Parameters);
            exp.Where(where);
            return exp;
        }

        public static SqlExpression<T> NotInArrayIfAny<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp>> selector, ICollection<TProp> ids)
        {
            return ids.Any() ? exp.NotInArray(selector, ids) : exp;
        }

        public static SqlExpression<T> InArray<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp?>> selector, IEnumerable<TProp> ids) where TProp : struct
        {
            var method = ContainsOfEnumerable.MakeGenericMethod(typeof(TProp));
            var argOfArr = Expression.Constant(ids, ids.GetType());
            var valueExp = Expression.Property(selector.Body, typeof(TProp?), nameof(Nullable<int>.Value));
            var callExp = Expression.Call(null, method, argOfArr, valueExp);
            var where = Expression.Lambda<Func<T, bool>>(callExp, selector.Parameters);
            exp.Where(where);
            return exp;
        }

        public static SqlExpression<T> NotInArray<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp?>> selector, IEnumerable<TProp> ids)
            where TProp : struct
        {
            var argOfArr = Expression.Constant(ids, ids.GetType());
            var method = ContainsOfEnumerable.MakeGenericMethod(typeof(TProp));
            var valueExp = Expression.Property(selector.Body, typeof(TProp?), nameof(Nullable<int>.Value));
            var callExp = Expression.Call(null, method, argOfArr, valueExp);
            var notExp = Expression.Not(callExp);
            var where = Expression.Lambda<Func<T, bool>>(notExp, selector.Parameters);
            exp.Where(where);
            return exp;
        }

        public static SqlExpression<T> NotInArrayIfAny<T, TProp>(this SqlExpression<T> exp,
            Expression<Func<T, TProp?>> selector, ICollection<TProp> ids)
            where TProp : struct
        {
            return ids.Any() ? exp.NotInArray(selector, ids) : exp;
        }
    }
}
