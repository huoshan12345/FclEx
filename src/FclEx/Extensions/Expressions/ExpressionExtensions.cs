using System;
using System.Linq.Expressions;
using FclEx.Utils;

namespace FclEx.Extensions.Expressions
{
    public static class ExpressionExtensions
    {
        public static TSource SetPropIf<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyLambda,
            Func<TSource, TProperty, bool> condition, TProperty newValue)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            var propertyInfo = ExpressionUtil.GetProp(propertyLambda);
            var value = propertyInfo.GetValue(source).CastTo<TProperty>();
            if (condition(source, value))
                propertyInfo.SetValue(source, newValue);
            return source;
        }

        public static TSource SetPropIf<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyLambda,
            Func<TProperty, bool> condition, TProperty newValue)
        {
            return SetPropIf(source, propertyLambda, (s, p) => condition(p), newValue);
        }

        public static TSource SetProp<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyLambda, TProperty newValue)
        {
            var propertyInfo = ExpressionUtil.GetProp(propertyLambda);
            propertyInfo.SetValue(source, newValue);
            return source;
        }

        public static TSource SetPropIfNull<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyLambda, TProperty newValue)
        {
            return SetPropIf(source, propertyLambda, (s, p) => p == null, newValue);
        }

        public static TSource SetPropIfNullOrEmpty<TSource>(this TSource source,
            Expression<Func<TSource, string>> propertyLambda, string newValue)
        {
            return SetPropIf(source, propertyLambda, (s, p) => p.IsNullOrEmpty(), newValue);
        }

        public static TSource SetPropIfDefault<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyLambda, TProperty newValue)
        {
            return SetPropIf(source, propertyLambda, (s, p) => p.IsDefault(), newValue);
        }

        public static TSource UpdatePropIf<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyLambda, TProperty newValue,
            Func<TProperty, bool> newValueCondition = null)
        {
            if (newValueCondition == null || newValueCondition(newValue))
            {
                var propertyInfo = ExpressionUtil.GetProp(propertyLambda);
                propertyInfo.SetValue(source, newValue);
            }
            return source;
        }

        public static TSource UpdatePropIfValid<TSource>(this TSource source,
            Expression<Func<TSource, string>> propertyLambda, string newValue)
        {
            return UpdatePropIf(source, propertyLambda, newValue, n => n.IsValid());
        }
    }
}
