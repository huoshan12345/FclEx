namespace Xunit;

public static partial class AssertExt
{
    private static readonly HashSet<string> _emptySet = [];

    private static readonly ConcurrentDictionary<Type, Func<object, object, bool>?> TypeEqualsDic = new();

    internal static Func<object, object, bool>? GetEqualsMethod(Type? type)
    {
        return type == null
            ? null
            : TypeEqualsDic.GetOrAdd(type, GetEqualsMethodInternal);

        static Func<object, object, bool>? GetEqualsMethodInternal(Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
                return Equals;

            if (type.IsInheritedFromGenericType(typeof(IEquatable<>)))
            {
                var method = type.GetMethod(nameof(IEquatable<object>.Equals), [type]);
                if (method != null)
                {
                    return (x, y) => (bool)method.Invoke(x, [y])!;
                }
            }
            return null;
        }
    }

    internal static TreeNode<ExcludeMember>? BuildExcludeMemberTree(string[] excludeMemberPaths)
    {
        if (excludeMemberPaths.IsNullOrEmpty())
            return null;

        var root = new TreeNode<ExcludeMember>(new ExcludeMember("$", false));

        var paths = excludeMemberPaths
            .Distinct()
            .OrderBy(m => m)
            .Select(m => m.Split('.'));

        foreach (var path in paths)
        {
            var cur = root;
            foreach (var (index, node) in path.Select((m, i) => (i, m)))
            {
                var child = cur.Children.FirstOrDefault(m => m.Value.Name == node);
                if (child == null)
                {
                    var isLast = (index + 1 == path.Length);
                    child = cur.AddChild(new ExcludeMember(node, isLast));
                }
                cur = child;
            }
        }

        return root;
    }

    internal static (object v1, object v2, Type? typeOfEqual) GetEqualType(Type t1, object v1, Type t2, object v2)
    {
        Type? t;
        if (TryGetTargetType(t1, t2, out var targetType))
        {
            t = targetType;
        }
        else if (IsEnumAndInteger(t1, t2))
        {
            // convert them to long to avoid loss of significance
            (v1, v2, t) = (v1.CastTo<long>(), v2.CastTo<long>(), typeof(long));
        }
        else if (t1.IsNumeric() && t2.IsNumeric())
        {
            // convert them to decimal to avoid loss of significance
            (v1, v2, t) = (v1.CastTo<decimal>(), v2.CastTo<decimal>(), typeof(decimal));
        }
        else
        {
            t = null;
        }
        return (v1, v2, t);

        static bool TryGetTargetType(Type t1, Type t2, out Type t)
        {
            if (t1 == t2)
            {
                t = t1;
                return true;
            }
            if (t1.IsAssignableFrom(t2))
            {
                t = t1;
                return true;
            }
            if (t2.IsAssignableFrom(t1))
            {
                t = t2;
                return true;
            }
            else
            {
                t = default!;
                return false;
            }
        }

        static bool IsEnumAndInteger(Type t1, Type t2)
        {
            return t1.IsEnum && t2.IsInteger() || t2.IsEnum && t1.IsInteger();
        }
    }

    internal readonly record struct EqualResult(bool Equal, object? Expected, object? Actual, string? Path, string? Banner = null)
    {
        private string CreateBanner()
        {
            var banner = Banner ?? "Values differ";
            return Path == null
                ? banner
                : banner + " at $" + Path;
        }

        public void ThrowIfNotEqual()
        {
            if (Equal == false)
                throw EqualException.ForMismatchedValues(Expected, Actual, CreateBanner());
        }

        public void ThrowIfEqual()
        {
            if (Equal)
                throw NotEqualException.ForEqualValues(Expected.ToStringOrEmpty(), Actual.ToStringOrEmpty(), CreateBanner());
        }
    }

    internal static EqualResult Equal(object? value1, object? value2, TreeNode<ExcludeMember>? excludeMemberTree, bool onlyCheckSameNameMembers, HashSet<(object, object)>? visited, string? currentPath)
    {
        if (value1 == null && value2 == null)
            return new(true, null, null, currentPath);

        if (value1 == null || value2 == null)
            return new(false, value1, value2, currentPath);

        if (ReferenceEquals(value1, value2))
            return new(true, value1, value2, currentPath);

        var (type1, type2) = (value1.GetType(), value2.GetType());

        visited ??= [];
        if (IsVisitableType(type1) && IsVisitableType(type2))
            visited.Add((value1, value2));

        (value1, value2, var typeOfEqual) = GetEqualType(type1, value1, type2, value2);

        var equalsMethod = GetEqualsMethod(typeOfEqual);
        if (equalsMethod != null)
            return new(equalsMethod(value1, value2), value1, value2, currentPath);

        if (type1.IsEnumerable() && type2.IsEnumerable())
        {
            // ReSharper disable once GenericEnumeratorNotDisposed
            using var disposable1 = ((IEnumerable)value1).GetEnumerator().ToDisposable();
            // ReSharper disable once GenericEnumeratorNotDisposed
            using var disposable2 = ((IEnumerable)value2).GetEnumerator().ToDisposable();

            var e1 = disposable1.Value;
            var e2 = disposable2.Value;

            for (var i = 0; ; ++i)
            {
                var b1 = e1.MoveNext();
                var b2 = e2.MoveNext();

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (!b1 && !b2)
                    return new(true, value1, value2, currentPath);

                if (b1 && b2)
                {
                    var path = currentPath + $"[{i}]";

                    var (v1, v2) = (e1.Current, e2.Current);
                    if (v1 == null && v2 == null)
                        continue;

                    if (v1 == null || v2 == null)
                        return new(false, value1, value2, path);

                    if (visited.Contains((v1, v2)))
                        continue;

                    var result = Equal(v1, v2, excludeMemberTree, onlyCheckSameNameMembers, visited, path);
                    if (!result.Equal)
                        return result;
                }
                else
                {
                    var count = i.ToString();
                    var (l, r) = (count, $"> {count}");
                    if (b1)
                        (l, r) = (r, l);

                    return new(false, l, r, currentPath, "Lengths differ");
                }
            }
        }

        if (type1 != type2)
        {
            var equal = false;
            var excludeNames = excludeMemberTree?.Children.Where(m => m.Value.IsExcluded).Select(m => m.Value.Name).ToHashSet() ?? _emptySet;
            var members1 = type1.GetDataMembers().Where(m => !excludeNames.Contains(m.Name)).ToList();
            var members2 = type2.GetDataMembers().Where(m => !excludeNames.Contains(m.Name)).ToList();

            if (!onlyCheckSameNameMembers && members1.Count != members2.Count)
                return new(false, value1, value2, currentPath);

            var members = from m1 in members1
                          join m2 in members2
                              on m1.Name equals m2.Name
                          select (m1.Name, m1, m2);

            foreach (var (name, m1, m2) in members)
            {
                var exclude = excludeMemberTree?.Children.FirstOrDefault(m => m.Value.Name == name);
                if (exclude?.Value.IsExcluded == true)
                    continue;

                var v1 = m1.GetValue(value1);
                var v2 = m2.GetValue(value2);

                if (v1 != null && v2 != null && visited.Contains((v1, v2)))
                    continue;

                var result = Equal(v1, v2, exclude, onlyCheckSameNameMembers, visited, currentPath + "." + name);
                if (result.Equal)
                    equal = true;
                else
                    return result;
            }
            return new(equal, value1, value2, currentPath);
        }
        else
        {
            var equal = false;
            var members = type1.GetDataMembers();
            foreach (var member in members)
            {
                var exclude = excludeMemberTree?.Children.FirstOrDefault(m => m.Value.Name == member.Name);
                if (exclude?.Value.IsExcluded == true)
                    continue;

                var v1 = member.GetValue(value1);
                var v2 = member.GetValue(value2);

                if (v1 != null && v2 != null && visited.Contains((v1, v2)))
                    continue;

                var result = Equal(v1, v2, exclude, onlyCheckSameNameMembers, visited, currentPath + "." + member.Name);
                if (result.Equal)
                    equal = true;
                else
                    return result;
            }
            return new(equal, value1, value2, currentPath);
        }

        static bool IsVisitableType(Type t)
        {
            return t.IsValueType == false && t != typeof(string);
        }
    }

    internal readonly struct ExcludeMember
    {
        public ExcludeMember(string name, bool isExcluded)
        {
            Name = name;
            IsExcluded = isExcluded;
        }

        public string Name { get; }
        public bool IsExcluded { get; }
    }
}