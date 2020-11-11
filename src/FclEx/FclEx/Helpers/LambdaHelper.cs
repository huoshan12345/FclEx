using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FclEx.Helpers
{
    public static class LambdaHelper<T>
    {
        public static LambdaExpression GetPropertyLambdaExp(string propertyName)
        {
            var param = Expression.Parameter(typeof(T));
            var body = Expression.Property(param, propertyName);
            var exp = Expression.Lambda(body, param);
            return exp;
        }
    }
}
