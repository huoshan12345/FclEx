namespace FclEx.Helpers;

public static class ObjectHelper
{
    public static object GetUninitializedObject(Type type)
    {
#if !NET5_0_OR_GREATER
        return FormatterServices.GetUninitializedObject(type);
#else
        return RuntimeHelpers.GetUninitializedObject(type);
#endif
    }

    public static T GetUninitializedObject<T>()
    {
        return (T)GetUninitializedObject(typeof(T));
    }

    private static long _nextId;
    // ReSharper disable once UseCollectionExpression
    private static readonly ConditionalWeakTable<object, object> _objectIds = new();
    public static long GetObjectId<T>(T? obj) where T : class
    {
        return obj is null
            ? 0
            : (long)_objectIds.GetValue(obj, _ => Interlocked.Increment(ref _nextId));
    }

    [MethodImpl(AggressiveInlining)]
    public static DisposableValue<GCHandle> ToGCHandle(object? obj, GCHandleType type)
    {
        return Disposable.FromValue(GCHandle.Alloc(obj, type), m => m.Free());
    }

    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CloneByJson<T>(T? obj, JsonSerializerOptions? options = null)
    {
        return obj is null ? obj : obj.ToJson(options).FromJson<T>(options);
    }

    public static byte[] MarshalToBytes<T>(T obj)
    {
        Check.NotNull(obj);

        var length = Marshal.SizeOf<T>();
        var bufByte = new byte[length];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        Marshal.StructureToPtr(obj, ptr, false);
        Marshal.Copy(ptr, bufByte, 0, length);
        return bufByte;
    }

    public static T? GetFieldValue<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetRequiredField(fieldName, true);
        return field.GetValue<T>(obj);
    }

    public static T GetRequiredFieldValue<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetRequiredField(fieldName, true);
        return field.GetRequiredValue<T>(obj);
    }

    private static readonly ConcurrentDictionary<MemberInfo, Delegate> _setterCache = new();
    private static readonly ConcurrentDictionary<MemberInfo, Delegate> _getterCache = new();

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
        var getter = (Func<T, TMember>)_getterCache.GetOrAdd(member, CreateGetter<T, TMember>);
        var setter = (RefAction<T, TMember>)_setterCache.GetOrAdd(member, CreateSetter<T, TMember>);

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
}