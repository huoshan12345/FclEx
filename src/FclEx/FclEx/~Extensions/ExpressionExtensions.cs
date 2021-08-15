using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>>? left, Expression<Func<T, bool>>? right)
        {
            if (left == null && right == null)
                throw new ArgumentNullException($"{nameof(left)}, {nameof(right)} cannot be null at the same time");
            if (left == null) return right!;
            if (right == null) return left;

            var parameter = left.Parameters[0];
            var r = ExpressionReplacer.Replace(right.Body, right.Parameters[0], parameter);
            var body = Expression.OrElse(left.Body, r);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return lambda;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>>? left, Expression<Func<T, bool>>? right)
        {
            if (left == null && right == null)
                throw new ArgumentNullException($"{nameof(left)}, {nameof(right)} cannot be null at the same time");
            if (left == null) return right!;
            if (right == null) return left;

            var parameter = left.Parameters[0];
            var r = ExpressionReplacer.Replace(right.Body, right.Parameters[0], parameter);
            var body = Expression.AndAlso(left.Body, r);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            return lambda;
        }

        public static Expression<TDelegate> Lambda<TDelegate>(this Expression e, params ParameterExpression[] parameters) where TDelegate : Delegate
            => Expression.Lambda<TDelegate>(e, parameters);

        public static Expression Convert(this Expression e, Type type) => Expression.Convert(e, type);

        public static LambdaExpression Lambda(this Expression e, params ParameterExpression[] parameters) => Expression.Lambda(e, parameters);

        public static void Visit(this BlockExpression block, Action<Expression> action)
        {
            foreach (var exp in block.Expressions)
            {
                if (exp is BlockExpression b)
                    Visit(b, action);
                else
                    action(exp);
            }
        }

        public static IEnumerable<object?> GetArgumentValues(this IEnumerable<Expression> arguments)
        {
            return arguments.Select(e => e switch
            {
                ConstantExpression constant => constant.Value,
                _ => e.Convert(typeof(object)).Lambda<Func<object>>().Compile().Invoke()
            });
        }
    }
}
