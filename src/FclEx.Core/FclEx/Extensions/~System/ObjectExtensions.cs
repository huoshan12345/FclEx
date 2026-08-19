namespace FclEx.Extensions;

public static partial class ObjectExtensions
{
    private static readonly ConditionalWeakTable<Type, ConditionalWeakTable<Type, Box<MethodInfo>>> _converterCache = new();

    private static MethodInfo? GetConversionOperator(Type fromType, Type toType)
    {
        var innerTable = _converterCache.GetOrCreateValue(fromType);

        if (innerTable.TryGetValue(toType, out var holder))
            return holder.Value;

        var method = toType.FindConversionOperator(fromType, toType)
                     ?? fromType.FindConversionOperator(fromType, toType);

        innerTable.GetValue(toType, m => new Box<MethodInfo>(method));

        return method;
    }
    
    /// <summary>
    /// Returns <paramref name="obj"/> when it is already <typeparamref name="T"/>, or converts it to that type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="obj">The source value.</param>
    /// <returns>The converted value, or the default value of <typeparamref name="T"/> when <paramref name="obj"/> is <see langword="null"/>.</returns>
    /// <remarks>
    /// After checking whether the value is already <typeparamref name="T"/>, this method searches the runtime source
    /// type and the target type for a public static <c>op_Implicit</c> or <c>op_Explicit</c> conversion operator.
    /// The target type is searched first. When no matching operator exists, non-enum conversions use
    /// <see cref="Convert.ChangeType(object, Type)"/> and enum conversions use <see cref="Enum.ToObject(Type, object)"/>.
    /// Conversion operators are discovered and invoked through reflection; applications that use trimming or Native AOT
    /// must preserve the applicable public operators. Exceptions thrown by an invoked operator are wrapped in
    /// <see cref="TargetInvocationException"/>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T? CastTo<T>(this object? obj)
    {
        return obj switch
        {
            null => default,
            T t => t,
            _ => ChangeType(obj),
        };

        static T ChangeType(object obj)
        {
            var type = typeof(T);
            var targetType = Nullable.GetUnderlyingType(type) ?? type;
            var sourceType = obj.GetType();

            var conversionOperator = GetConversionOperator(sourceType, targetType);

            if (conversionOperator is not null)
                return conversionOperator.Invoke<T>(null, [obj])!;

            return targetType.IsEnum
                ? (T)Enum.ToObject(targetType, obj)
                : (T)Convert.ChangeType(obj, targetType);
        }
    }

    /// <summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
    /// <exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        Check.NotGreaterThan(min, max);

        var cmpMin = value.CompareTo(min);
        if (cmpMin <= 0) // value <= min
            return min;

        var cmpMax = value.CompareTo(max);
        return cmpMax >= 0 ? // value >= max
            max : value;
    }

    [MethodImpl(AggressiveInlining)]
    public static T? ToNullable<T>(this T value) where T : struct
    {
        return value;
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

    private static long _nextId;
    private static readonly ConditionalWeakTable<object, object> _objectIds = new();

    extension(object)
    {
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

            var member = Expression.GetDataMember(selector);
            var getter = AccessorCache<T, TMember>.Getters.GetOrAdd(member, CreateGetter<T, TMember>);
            var setter = AccessorCache<T, TMember>.Setters.GetOrAdd(member, CreateSetter<T, TMember>);

            var oldValue = getter(obj);

            comparer ??= EqualityComparer<TMember>.Default;

            if (comparer.Equals(oldValue, newValue))
                return false;

            setter(ref obj, newValue);
            return true;
        }
    }
}
