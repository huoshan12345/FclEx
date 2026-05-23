namespace System.Collections.Generic;

public class MemberEqualityComparerBuilder
{
    public static MemberEqualityComparerBuilder<T> Create<T>()
    {
        return MemberEqualityComparerBuilder<T>.Create();
    }
}

public class MemberEqualityComparerBuilder<T> : IEqualityComparerBuilder<T>
{
    private readonly List<IEqualityMember> _members = [];

    public static MemberEqualityComparerBuilder<T> Create()
    {
        return new MemberEqualityComparerBuilder<T>();
    }

    public MemberEqualityComparerBuilder<T> Add<TMember>(Func<T, TMember?> selector, IEqualityComparer<TMember>? memberComparer = null)
    {
        _members.Add(new EqualityMember<TMember>(selector, memberComparer ?? EqualityComparer<TMember>.Default));
        return this;
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
                var equal = member.Equals(x, y);
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

    private interface IEqualityMember
    {
        bool Equals(T x, T y);
        int GetHashCode(T obj);
    }

    private sealed class EqualityMember<TMember> : IEqualityMember
    {
        private readonly Func<T, TMember?> _selector;
        private readonly IEqualityComparer<TMember> _comparer;

        public EqualityMember(Func<T, TMember?> selector, IEqualityComparer<TMember> comparer)
        {
            _selector = selector;
            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            var vx = _selector(x);
            var vy = _selector(y);
            return ComparerHelper.TryEquals(vx, vy, out var result)
                ? result.Value
                : _comparer.Equals(vx, vy);
        }

        public int GetHashCode(T obj)
        {
            var value = _selector(obj);
            return value is null
                ? 0
                : _comparer.GetHashCode(value);
        }
    }
}