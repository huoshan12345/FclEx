namespace FclEx.Comparers;

public class MemberEqualityComparerBuilder
{
    public static MemberEqualityComparerBuilder<T> Create<T>()
    {
        return MemberEqualityComparerBuilder<T>.Create();
    }
}

public class MemberEqualityComparerBuilder<T> : IEqualityComparerBuilder<T>
{
    private readonly List<EqualityMember> _members = [];

    public static MemberEqualityComparerBuilder<T> Create()
    {
        return new MemberEqualityComparerBuilder<T>();
    }

    public MemberEqualityComparerBuilder<T> Add<TMember>(Func<T, TMember?> selector, IEqualityComparer<TMember>? memberComparer = null)
    {
        IEqualityComparer comparer = memberComparer == null
            ? EqualityComparer<TMember>.Default
            : UntypedEqualityComparer.Create(memberComparer);

        var prop = new EqualityMember(m => selector(m), comparer);
        _members.Add(prop);
        return this;
    }

    private static bool Equals(T x, T y, EqualityMember member)
    {
        var left = member.Selector(x);
        var right = member.Selector(y);
        return member.EqualityComparer.Equals(left, right);
    }

    public Func<T?, T?, bool> CreateCompareFunc()
    {
        return (x, y) =>
        {
            if (ComparerHelper.TryEquals(x, y, out var result))
                return result.Value;

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var member in _members)
            {
                var equal = Equals(x, y, member);
                if (equal == false)
                    return false;
            }
            return true;
        };
    }

    public Func<T, int> CreateHashFunc()
    {
        return x =>
        {
            var hash = new HashCode();
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var member in _members)
            {
                var code = member.GetHashCode(x);
                hash.Add(code);
            }
            return hash.ToHashCode();
        };
    }

    public IEqualityComparer<T> Build()
    {
        return new CommonEqualityComparer<T>(CreateCompareFunc(), CreateHashFunc());
    }

    private readonly record struct EqualityMember(Func<T, object?> Selector, IEqualityComparer EqualityComparer)
    {
        public int GetHashCode(T obj)
        {
            var value = Selector(obj);
            return value is null
                ? 0
                : EqualityComparer.GetHashCode(value);
        }
    }
}

public static class MemberEqualityComparerBuilderExtensions
{
    private static bool CanRead(this DataMemberInfo member, bool includeNonPublic)
    {
        return member.CanRead && (includeNonPublic || member.HasPublicGetter);
    }

    private static MemberExpression ToExpression(this DataMemberInfo member, Expression expression)
    {
        return member.MemberInfo is PropertyInfo property
            ? Expression.Property(expression, property)
            : Expression.Field(expression, member.MemberInfo.CastTo<FieldInfo>());
    }

    public static MemberEqualityComparerBuilder<T> AddAllPublicDataMembers<T>(this MemberEqualityComparerBuilder<T> builder, params Expression<Func<T, object?>>[] excludeMemberSelectors)
    {
        return builder.AddAllDataMembers(false, excludeMemberSelectors);
    }

    public static MemberEqualityComparerBuilder<T> AddAllDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        bool includeNonPublic = false, params Expression<Func<T, object?>>[] excludeMemberSelectors)
    {
        var exclude = excludeMemberSelectors
            .Select(m => ExpressionHelper.GetDataMemberInfo(m))
            .ToHashSet();

        var paramExp = Expression.Parameter(typeof(T));

        foreach (var member in typeof(T).GetDataMembers().Where(m => m.CanRead(includeNonPublic)))
        {
            if (exclude.Contains(member))
                continue;

            var memberExp = member.ToExpression(paramExp);
            var convert = Expression.Convert(memberExp, typeof(object));
            var func = convert.Lambda<Func<T, object?>>(paramExp).Compile();
            builder.Add(func);
        }
        return builder;
    }
}