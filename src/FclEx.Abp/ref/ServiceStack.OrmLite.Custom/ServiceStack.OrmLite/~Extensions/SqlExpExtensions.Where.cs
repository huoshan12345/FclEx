using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.Extensions;
using static ServiceStack.OrmLite.CommonMethods;

namespace ServiceStack.OrmLite
{
    public static partial class SqlExpExtensions
    {
        internal static Expression<Func<T, bool>> BuildStringFilter<T>(Expression<Func<T, string>> selector, MethodInfo methodInfo, string value)
        {
            var someValue = Expression.Constant(value, typeof(string));
            var call = Expression.Call(selector.Body, methodInfo, someValue);
            var where = Expression.Lambda<Func<T, bool>>(call, selector.Parameters);
            return where;
        }

        internal static Expression<Func<T, bool>> BuildContains<T>(Expression<Func<T, string>> selector, string value)
        {
            return BuildStringFilter(selector, ContainsOfString, value);
        }

        internal static Expression<Func<T, bool>> BuildStartsWith<T>(Expression<Func<T, string>> selector, string value)
        {
            return BuildStringFilter(selector, StartsWith, value);
        }


        public static SqlExpression<T> WhereIf<T>(this SqlExpression<T> exp, bool condition, Expression<Func<T, bool>> where)
        {
            return condition ? exp.Where(where) : exp;
        }

        public static SqlExpression<T> WhereIf<T, TTarget>(this SqlExpression<T> exp, bool condition, Expression<Func<TTarget, bool>> where)
        {
            return condition ? exp.Where(where) : exp;
        }

        public static SqlExpression<T> ContainsIfValid<T>(this SqlExpression<T> exp, Expression<Func<T, string>> selector, string value)
        {
            if (value.IsValid())
            {
                var where = BuildContains(selector, value);
                exp = exp.Where(where);
            }
            return exp;
        }

        public static SqlExpression<T> Where<T>(this SqlExpression<T> exp, Expression<Func<SqlExpression<T>, T, bool>> predicate)
        {
            var (body, paras) = exp.CreateExp(predicate);
            var newSelector = Expression.Lambda<Func<T, bool>>(((LambdaExpression)body).Body, paras);
            return exp.Where(newSelector);
        }

        public static SqlExpression<T> ContainsAny<T>(this SqlExpression<T> exp, Expression<Func<T, string>> selector, IEnumerable<string> keywords)
        {
            var where = keywords.Select(keyword => BuildContains(selector, keyword))
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current, contains) => ExpressionExtensions.Or(current, contains));
            return where == null ? exp : exp.Where(where);
        }

        public static SqlExpression<T> StartsWithAny<T>(this SqlExpression<T> exp, Expression<Func<T, string>> selector, IEnumerable<string> keywords)
        {
            var where = keywords.Select(keyword => BuildStartsWith(selector, keyword))
                .Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>(null,
                    (current, contains) => ExpressionExtensions.Or(current, contains));
            return where == null ? exp : exp.Where(where);
        }
    }
}
