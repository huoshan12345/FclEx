namespace System.Collections.Generic;

public class MemberComparerBuilder
{
    public static MemberComparerBuilder<T> Create<T>()
    {
        return MemberComparerBuilder<T>.Create();
    }
}

public class MemberComparerBuilder<T> : IComparerBuilder<T>
{
    private readonly List<OrderMember> _members = [];

    public static MemberComparerBuilder<T> Create()
    {
        return new();
    }

    public static MemberComparerBuilder<T> Create<TMember>(Func<T, TMember?> selector, bool desc = false, IComparer<TMember>? memberComparer = null)
    {
        return new MemberComparerBuilder<T>()
            .OrderBy(selector, desc, memberComparer);
    }

    public MemberComparerBuilder<T> OrderBy<TMember>(Func<T, TMember?> selector, bool desc = false, IComparer<TMember>? memberComparer = null)
    {
        IComparer comparer = memberComparer == null
            ? Comparer<TMember>.Default
            : NonGenericComparerAdapter.Create(memberComparer);

        var member = new OrderMember(m => selector(m), desc, comparer);
        _members.Add(member);
        return this;
    }

    private static int Compare(T x, T y, OrderMember member)
    {
        var left = member.Selector(x);
        var right = member.Selector(y);
        return member.Desc
            ? member.Comparer.Compare(right, left)
            : member.Comparer.Compare(left, right);
    }

    public Comparison<T?> CreateComparison()
    {
        return (x, y) =>
        {
            if (ComparerHelper.TryCompare(x, y, out var result))
                return result.Value;

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var member in _members)
            {
                var cmp = Compare(x, y, member);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        };
    }

    public IComparer<T> Build() => CommonComparer.Create(CreateComparison());

    private readonly record struct OrderMember(Func<T, object?> Selector, bool Desc, IComparer Comparer);
}