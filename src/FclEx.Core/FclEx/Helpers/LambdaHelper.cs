namespace FclEx.Helpers;

public static class LambdaHelper
{
    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<string, LambdaExpression>> _cache = new();

    public static LambdaExpression GetPropertyLambdaExp<T>(string propertyName)
    {
        var type = typeof(T);
        var expressions = _cache.GetValue(type, _ => new());
        return expressions.GetOrAdd(propertyName, name =>
        {
            var param = Expression.Parameter(type);
            var body = Expression.Property(param, name);
            var exp = Expression.Lambda(body, param);
            return exp;
        });
    }
}
