using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FclEx.Helpers
{
    public static class ExpressionHelper
    {
        public static PropertyInfo GetProp<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            if (propertyLambda == null) throw new ArgumentNullException(nameof(propertyLambda));

            if (!(propertyLambda.Body is MemberExpression member))
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            if (!(member.Member is PropertyInfo propInfo))
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

            //If the MemberInfo object is a global member (that is, if it was obtained from the Module.GetMethods method,
            //which returns global methods on a module), the returned DeclaringType will be null.
            if (propInfo.ReflectedType == null)
                throw new ArgumentException($"Expression '{propertyLambda}' does not refer to a property of a class.");

            var type = typeof(TSource);
            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a property that is not from type {type}.");

            return propInfo;
        }

        public static Expression<Func<T, object>> ErasureType<T, TProp>(Expression<Func<T, TProp>> selector)
        {
            var type = typeof(TProp);
            if (type != typeof(object))
            {
                var e = Expression.Convert(selector.Body, typeof(object));
                return Expression.Lambda<Func<T, object>>(e, selector.Parameters);
            }
            else
            {
                return Expression.Lambda<Func<T, object>>(selector.Body, selector.Parameters);
            }
        }
    }
}
