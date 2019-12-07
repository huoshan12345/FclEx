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
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            if (left == null && right == null)
                throw new ArgumentNullException($"{nameof(left)}, {nameof(right)} cannot be null at the same time");
            if (left == null) return right;
            if (right == null) return left;

            var parameter = left.Parameters[0];
            var r = ExpressionReplacer.Replace(right.Body, right.Parameters[0], parameter);
            var body = Expression.OrElse(left.Body, r);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return lambda;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            if (left == null && right == null)
                throw new ArgumentNullException($"{nameof(left)}, {nameof(right)} cannot be null at the same time");
            if (left == null) return right;
            if (right == null) return left;

            var parameter = left.Parameters[0];
            var r = ExpressionReplacer.Replace(right.Body, right.Parameters[0], parameter);
            var body = Expression.AndAlso(left.Body, r);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return lambda;
        }
    }
}
