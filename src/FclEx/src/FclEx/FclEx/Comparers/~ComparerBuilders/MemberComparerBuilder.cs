namespace FclEx.Comparers;

public class MemberComparerBuilder
{
    public static MemberComparerBuilder<T> Create<T>(bool isNullSmaller = true)
    {
        return MemberComparerBuilder<T>.Create(isNullSmaller);
    }
}

public class MemberComparerBuilder<T> : IComparerBuilder<T>
{
    private readonly bool _isNullSmaller;
    private readonly IList<OrderMember> _members = new List<OrderMember>();

    public MemberComparerBuilder(bool isNullSmaller = true)
    {
        _isNullSmaller = isNullSmaller;
    }

    public static MemberComparerBuilder<T> Create(bool isNullSmaller = true)
    {
        return new MemberComparerBuilder<T>(isNullSmaller);
    }

    public static MemberComparerBuilder<T> Create<TMember>(Func<T, TMember?> selector, bool desc = false, IComparer<TMember>? memberComparer = null, bool isNullSmaller = true)
    {
        return new MemberComparerBuilder<T>(isNullSmaller)
            .OrderBy(selector, desc, memberComparer);
    }

    public MemberComparerBuilder<T> OrderBy<TMember>(Func<T, TMember?> selector, bool desc = false, IComparer<TMember>? memberComparer = null)
    {
        memberComparer ??= Comparer<TMember>.Default;

        IComparer comparer = memberComparer == null
            ? Comparer<TMember>.Default
            : UntypedComparer.Create(memberComparer);

        var prop = new OrderMember(m => selector(m), desc, comparer);
        _members.Add(prop);
        return this;
    }

    private int Compare(T? x, T? y, OrderMember member)
    {
        if (ComparerHelper.TryCompare(x, y, _isNullSmaller, out var result))
            return result.Value;

        var left = member.Selector(x);
        var right = member.Selector(y);
        return member.Desc
            ? member.Comparer.Compare(right, left)
            : member.Comparer.Compare(left, right);
    }

    public Comparison<T?> ToComparison()
    {
        return (x, y) =>
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var member in _members)
            {
                var cmp = Compare(x, y, member);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        };
    }

    public IComparer<T> Build() => CommonComparer.Create(ToComparison());

    private readonly record struct OrderMember(Func<T, object?> Selector, bool Desc, IComparer Comparer);
}