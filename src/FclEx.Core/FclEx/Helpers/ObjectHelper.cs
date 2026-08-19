namespace FclEx.Helpers;

public static class ObjectHelper
{
    private static long _nextId;
    // ReSharper disable once UseCollectionExpression
    private static readonly ConditionalWeakTable<object, object> _objectIds = new();
    public static long GetObjectId<T>(T? obj) where T : class
    {
        return obj is null
            ? 0
            : (long)_objectIds.GetValue(obj, _ => Interlocked.Increment(ref _nextId));
    }

    public static bool TrySet<T, TMember>(T obj, Expression<Func<T, TMember>> selector,
        TMember newValue, IEqualityComparer<TMember>? comparer = null)
        where T : class
    {
        return TrySet(ref obj, selector, newValue, comparer);
    }

    public static bool TrySet<T, TMember>(ref T obj, Expression<Func<T, TMember>> selector,
        TMember newValue, IEqualityComparer<TMember>? comparer = null)
    {
        Check.NotNull(obj);
        Check.NotNull(selector);

        var member = ExpressionHelper.GetDataMember(selector);
        var getter = AccessorCache<T, TMember>.Getters.GetOrAdd(member, CreateGetter<T, TMember>);
        var setter = AccessorCache<T, TMember>.Setters.GetOrAdd(member, CreateSetter<T, TMember>);

        var oldValue = getter(obj);

        comparer ??= EqualityComparer<TMember>.Default;

        if (comparer.Equals(oldValue, newValue))
            return false;

        setter(ref obj, newValue);
        return true;
    }

    private static Func<T, TMember> CreateGetter<T, TMember>(MemberInfo member)
    {
        var objParam = Expression.Parameter(typeof(T));

        Expression memberAccess = member switch
        {
            PropertyInfo prop => Expression.Property(objParam, prop),
            FieldInfo field => Expression.Field(objParam, field),
            _ => throw new InvalidOperationException($"Member {member.Name} is neither property nor field")
        };

        return Expression
            .Lambda<Func<T, TMember>>(memberAccess, objParam)
            .Compile();
    }

    private static RefAction<T, TMember> CreateSetter<T, TMember>(MemberInfo member)
    {
        var objParam = Expression.Parameter(typeof(T).MakeByRefType());
        var valueParam = Expression.Parameter(typeof(TMember));

        Expression memberAccess = member switch
        {
            PropertyInfo { CanWrite: true } prop when prop.IsInitOnly() == false => Expression.Property(objParam, prop),
            FieldInfo { IsInitOnly: false } field => Expression.Field(objParam, field),
            _ => throw new InvalidOperationException($"Member {member.Name} is not writable")
        };

        var assign = Expression.Assign(memberAccess, valueParam);

        return Expression
            .Lambda<RefAction<T, TMember>>(assign, objParam, valueParam)
            .Compile();
    }

    private static class AccessorCache<T, TMember>
    {
        public static readonly ConcurrentDictionary<MemberInfo, Func<T, TMember>> Getters = new();
        public static readonly ConcurrentDictionary<MemberInfo, RefAction<T, TMember>> Setters = new();
    }
}
