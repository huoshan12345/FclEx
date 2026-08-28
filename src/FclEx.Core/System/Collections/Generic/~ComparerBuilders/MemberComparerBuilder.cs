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
    private readonly List<IOrderMember> _members = [];

    public static MemberComparerBuilder<T> Create()
    {
        return new();
    }

    public static MemberComparerBuilder<T> Create<TMember>(Func<T, TMember?> selector, bool desc = false, IComparer<TMember>? memberComparer = null)
    {
        return new MemberComparerBuilder<T>()
            .OrderBy(selector, desc, memberComparer);
    }

    public static MemberComparerBuilder<T> Create<TMember>(Func<T, TMember?> selector, IComparer<TMember>? memberComparer, bool desc = false)
    {
        return Create(selector, desc, memberComparer);
    }

    public MemberComparerBuilder<T> OrderBy<TMember>(Func<T, TMember?> selector, bool desc = false, IComparer<TMember>? memberComparer = null)
    {
        _members.Add(new OrderMember<TMember>(selector, desc, memberComparer ?? Comparer<TMember>.Default));
        return this;
    }

    public MemberComparerBuilder<T> OrderBy<TMember>(Func<T, TMember?> selector, IComparer<TMember>? memberComparer, bool desc = false)
    {
        return OrderBy(selector, desc, memberComparer);
    }

    public Comparison<T?> CreateComparison()
    {
        return (x, y) =>
        {
            if (Comparer.TryCompare(x, y, out var result))
                return result.Value;

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var m in _members)
            {
                var cmp = m.Compare(x, y);
                if (cmp != 0)
                    return cmp;
            }
            return 0;
        };
    }

    public IComparer<T> Build() => DelegateComparer.Create(CreateComparison());

    private interface IOrderMember
    {
        int Compare(T x, T y);
    }

    private sealed class OrderMember<TMember> : IOrderMember
    {
        private readonly Func<T, TMember?> _selector;
        private readonly IComparer<TMember> _comparer;
        private readonly bool _desc;

        public OrderMember(Func<T, TMember?> selector, bool desc, IComparer<TMember> comparer)
        {
            _selector = selector;
            _desc = desc;
            _comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            var l = _selector(x);
            var r = _selector(y);

            if (Comparer.TryCompare(l, r, out var result))
                return _desc
                    ? -result.Value
                    : result.Value;

            return _desc
                ? _comparer.Compare(r, l)
                : _comparer.Compare(l, r);
        }
    }
}
