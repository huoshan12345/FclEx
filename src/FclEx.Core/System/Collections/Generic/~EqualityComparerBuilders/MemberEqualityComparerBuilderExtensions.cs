using static System.Reflection.DataMemberFlags;

namespace System.Collections.Generic;

public static class MemberEqualityComparerBuilderExtensions
{
    private static MemberExpression ToExpression(this DataMemberInfo member, Expression expression)
    {
        return member.MemberInfo is PropertyInfo property
            ? Expression.Property(expression, property)
            : Expression.Field(expression, member.MemberInfo.CastTo<FieldInfo>());
    }

    public static MemberEqualityComparerBuilder<T> Add<T, TMember>(this MemberEqualityComparerBuilder<T> builder,
        string name, IEqualityComparer<TMember>? memberComparer)
    {
        var member = typeof(T).GetRequiredDataMember(name);
        var paramExp = Expression.Parameter(typeof(T));
        var memberExp = member.ToExpression(paramExp);
        var func = memberExp.Lambda<Func<T, TMember>>(paramExp).Compile();
        builder.Add(func, memberComparer);
        return builder;
    }

    public static MemberEqualityComparerBuilder<T> Add<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<string> names)
    {
        var paramExp = Expression.Parameter(typeof(T));
        foreach (var name in names)
        {
            var member = typeof(T).GetRequiredDataMember(name);
            var memberExp = member.ToExpression(paramExp);
            var convert = Expression.Convert(memberExp, typeof(object));
            var func = convert.Lambda<Func<T, object?>>(paramExp).Compile();
            builder.Add(func);
        }
        return builder;
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

        const DataMemberFlags publicFlags = Declared | Inherited | CanRead | Property | Field | Instance | Public;
        const DataMemberFlags allFlags = publicFlags | NonPublic;

        var flags = includeNonPublic
            ? allFlags
            : publicFlags;

        foreach (var member in typeof(T).GetDataMembers(flags))
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