using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FclEx.Helpers
{
    public static class LambdaHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), LambdaExpression> _cache = new();

        public static LambdaExpression GetPropertyLambdaExp<T>(string propertyName)
        {
            return _cache.GetOrAdd((typeof(T), propertyName), k =>
            {
                var param = Expression.Parameter(k.Item1);
                var body = Expression.Property(param, k.Item2);
                var exp = Expression.Lambda(body, param);
                return exp;
            });
        }
    }
}
