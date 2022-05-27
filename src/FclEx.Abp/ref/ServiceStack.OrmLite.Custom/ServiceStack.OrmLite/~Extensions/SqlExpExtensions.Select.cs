using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FclEx.Utils;

namespace ServiceStack.OrmLite
{
    public static partial class SqlExpExtensions
    {
        public static (Expression Body, IEnumerable<ParameterExpression> Paras) CreateExp(
            this ISqlExpression exp, LambdaExpression selector)
        {
            var para = Expression.Constant(exp);
            var paras = selector.Parameters;
            var newExp = ExpressionReplacer.Replace(selector.Body, paras.First(), para);
            var newParas = paras.Skip(1);
            return (newExp, newParas);
        }

        public static SqlExpression<T> Select<T>(this SqlExpression<T> exp,
            Expression<Func<SqlExpression<T>, T, object>> selector)
        {
            var (body, paras) = CreateExp(exp, selector);
            var newSelector = Expression.Lambda<Func<T, object>>(body, paras);
            return exp.Select(newSelector);
        }

        public static SqlExpression<T> Select<T, TTable>(this SqlExpression<T> exp,
            Expression<Func<SqlExpression<T>, T, TTable, object>> selector)
        {
            var (body, paras) = CreateExp(exp, selector);
            var newSelector = Expression.Lambda<Func<T, TTable, object>>(body, paras);
            return exp.Select(newSelector);
        }

        public static SqlExpression<T> Select<T, TTable1, TTable2>(this SqlExpression<T> exp,
            Expression<Func<SqlExpression<T>, T, TTable1, TTable2, object>> selector)
        {
            var (body, paras) = CreateExp(exp, selector);
            var newSelector = Expression.Lambda<Func<T, TTable1, TTable2, object>>(body, paras);
            return exp.Select(newSelector);
        }
    }
}
