namespace FclEx.Extensions;

public static class LambdaExpressionExtensions
{
    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<string, LambdaExpression>> _cache = new();

    extension(LambdaExpression)
    {
        public static LambdaExpression PropertyOrField(Type type, string propertyOrFieldName)
        {
            var expressions = _cache.GetValue(type, _ => new());
            return expressions.GetOrAdd(propertyOrFieldName, name =>
            {
                var param = Expression.Parameter(type);
                var body = Expression.PropertyOrField(param, name);
                var exp = Expression.Lambda(body, param);
                return exp;
            });
        }

        public static LambdaExpression PropertyOrField<T>(string propertyOrFieldName)
        {
            return LambdaExpression.PropertyOrField(typeof(T), propertyOrFieldName);
        }
    }
}
