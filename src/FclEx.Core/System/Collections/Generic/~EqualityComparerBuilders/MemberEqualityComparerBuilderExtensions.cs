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

    public static MemberEqualityComparerBuilder<T> Add<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<DataMemberInfo> members)
    {
        var addMethod = typeof(MemberEqualityComparerBuilder<T>)
            .GetMethods()
            .Single(x =>
                x is { Name: nameof(MemberEqualityComparerBuilder<T>.Add), IsGenericMethodDefinition: true }
                && x.GetParameters().Length == 2);

        var param = Expression.Parameter(typeof(T));

        foreach (var member in members)
        {
            var memberType = member.DataMemberType;
            var access = member.ToExpression(param);
            var funcType = typeof(Func<,>).MakeGenericType(typeof(T), memberType);
            var lambda = Expression.Lambda(funcType, access, param).Compile();
            var genericAdd = addMethod.MakeGenericMethod(memberType);
            genericAdd.Invoke(builder, [lambda, null]);
        }

        return builder;
    }

    public static MemberEqualityComparerBuilder<T> Add<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<FieldInfo> members)
    {
        return builder.Add(members.Select(m => m.ToDataMemberInfo()));
    }

    public static MemberEqualityComparerBuilder<T> Add<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<PropertyInfo> members)
    {
        return builder.Add(members.Select(m => m.ToDataMemberInfo()));
    }

    public static MemberEqualityComparerBuilder<T> Add<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<string> names)
    {
        var type = typeof(T);
        return builder.Add(names.Select(type.GetRequiredDataMember));
    }
    
    public static MemberEqualityComparerBuilder<T> AddAllPublicDataMembers<T>(this MemberEqualityComparerBuilder<T> builder, params IEnumerable<string> excludeMemberNames)
    {
        return builder.AddAllDataMembers(false, excludeMemberNames);
    }

    public static MemberEqualityComparerBuilder<T> AddAllDataMembers<T>(this MemberEqualityComparerBuilder<T> builder,
        bool includeNonPublic = false, params IEnumerable<string> excludeMemberNames)
    {
        const DataMemberFlags publicFlags = Declared | Inherited | CanRead | Property | Field | Instance | Public;
        const DataMemberFlags allFlags = publicFlags | NonPublic;

        var flags = includeNonPublic
            ? allFlags
            : publicFlags;

        var set = excludeMemberNames.AsISet();
        var members = typeof(T).GetDataMembers(flags)
            .Where(m => set.Contains(m.Name) == false);

        return builder.Add(members);
    }
}