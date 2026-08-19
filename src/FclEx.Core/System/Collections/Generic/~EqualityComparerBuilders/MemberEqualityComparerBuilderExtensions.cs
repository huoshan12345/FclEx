using static System.Reflection.DataMemberFlags;

namespace System.Collections.Generic;

public static class MemberEqualityComparerBuilderExtensions
{
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

    private static readonly ConditionalWeakTable<Type, MethodInfo> _addMethodCache = new();

    private static MethodInfo GetAddMethod(Type builderType)
    {
        return _addMethodCache.GetValue(builderType, t =>
        {
            var method = typeof(MemberEqualityComparerBuilder<>)
                .MakeGenericType(t)
                .GetMethods()
                .Single(x =>
                    x is { Name: nameof(MemberEqualityComparerBuilder<>.Add), IsGenericMethodDefinition: true }
                    && x.GetParameters().Length == 2);
            return method;
        });
    }

    public static MemberEqualityComparerBuilder<T> Add<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<MemberInfo> members)
    {
        var type = typeof(T);
        var addMethod = GetAddMethod(type);
        var param = Expression.Parameter(type);

        foreach (var member in members)
        {
            var (access, memberType) = member switch
            {
                PropertyInfo property => (Expression.Property(param, property), property.PropertyType),
                FieldInfo field => (Expression.Field(param, field), field.FieldType),
                DataMemberInfo dataMember => (dataMember.ToExpression(param), dataMember.DataMemberType),
                _ => throw new ArgumentException($"{member.GetType().ShortName()} '{member.Name}' is not a field or property.", nameof(members))
            };
            var funcType = typeof(Func<,>).MakeGenericType(typeof(T), memberType);
            var lambda = Expression.Lambda(funcType, access, param).Compile();
            var genericAdd = addMethod.MakeGenericMethod(memberType);
            genericAdd.Invoke(builder, [lambda, null]);
        }

        return builder;
    }

    public static MemberEqualityComparerBuilder<T> Add<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<string> names)
    {
        var type = typeof(T);
        return builder.Add(names.Select(type.GetRequiredDataMember));
    }

    public static MemberEqualityComparerBuilder<T> AddAllPublicDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        params IEnumerable<MemberInfo> excludeMembers)
    {
        return builder.AddAllDataMembers(false, excludeMembers);
    }

    public static MemberEqualityComparerBuilder<T> AddAllPublicDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        params IEnumerable<string> excludeMemberNames)
    {
        return builder.AddAllDataMembers(false, excludeMemberNames);
    }

    public static MemberEqualityComparerBuilder<T> AddAllPublicDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        params Expression<Func<T, object?>>[] members)
    {
        return builder.AddAllDataMembers(false, members);
    }

    public static MemberEqualityComparerBuilder<T> AddAllDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        bool includeNonPublic = false, params IEnumerable<MemberInfo> excludeMembers)
    {
        const DataMemberFlags publicFlags = Declared | Inherited | CanRead | Property | Field | Instance | Public;
        const DataMemberFlags allFlags = publicFlags | NonPublic;

        var flags = includeNonPublic
            ? allFlags
            : publicFlags;

        var set = excludeMembers.ToHashSet(MemberInfoEqualityComparer.Instance);
        var members = typeof(T).GetDataMembers(flags)
            .Where(m => set.Contains(m.MemberInfo) == false);

        return builder.Add(members);
    }

    public static MemberEqualityComparerBuilder<T> AddAllDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        bool includeNonPublic = false, params IEnumerable<string> excludeMemberNames)
    {
        var type = typeof(T);
        return builder.AddAllDataMembers(includeNonPublic, excludeMemberNames.Select(m => type.GetRequiredDataMember(m).MemberInfo));
    }

    public static MemberEqualityComparerBuilder<T> AddAllDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        bool includeNonPublic = false, params Expression<Func<T, object?>>[] members)
    {
        var memberInfos = members.SelectMany(Expression.GetDataMembers);
        return builder.AddAllDataMembers(includeNonPublic, memberInfos);
    }
}
