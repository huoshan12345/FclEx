using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx
{
    public static class ExpressionExtensions
    {
        private static Expression<T> Compose<T>(this Expression<T> left,
            Expression<T> right, Func<Expression, Expression, Expression> merge)
        {
            if (left == null) return right;
            var invExpr = Expression.Invoke(right, left.Parameters);
            return Expression.Lambda<T>(merge(left.Body, invExpr), left.Parameters);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            return left.Compose(right, Expression.OrElse);
        }

        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> left,
            bool condition, Expression<Func<T, bool>> right)
        {
            return condition ? Or(left, right) : left;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            return left.Compose(right, Expression.AndAlso);
        }

        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> left,
            bool condition, Expression<Func<T, bool>> right)
        {
            return condition ? And(left, right) : left;
        }
    }
}
