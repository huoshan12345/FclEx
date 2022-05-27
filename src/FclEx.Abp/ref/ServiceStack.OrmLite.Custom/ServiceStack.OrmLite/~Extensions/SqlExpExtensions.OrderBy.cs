using System;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public static partial class SqlExpExtensions
    {
        public static SqlExpression<T> OrderBy<T>(this SqlExpression<T> exp,
            Expression<Func<SqlExpression<T>, T, object>> selector)
        {
            var (body, paras) = exp.CreateExp(selector);
            var newSelector = Expression.Lambda<Func<T, object>>(body, paras);
            return exp.OrderBy(newSelector);
        }
    }
}
