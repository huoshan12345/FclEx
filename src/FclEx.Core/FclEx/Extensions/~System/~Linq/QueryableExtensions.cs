namespace FclEx.Extensions; 

public readonly record struct JoinResult<TOuter, TInner>(TOuter Outer, TInner Inner);

public static partial class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool condition)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Expression<Func<T, int, bool>> predicate, bool condition)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static IQueryable<JoinResult<TOuter, TInner>> Join<TOuter, TInner, TKey>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector)
    {
        return outer.Join(inner, outerKeySelector, innerKeySelector, (m, n) => new JoinResult<TOuter, TInner>(m, n));
    }
}