using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FclEx;
using FclEx.Utils;

namespace Xunit
{
    public static partial class AssertExt
    {
        private static readonly HashSet<string> _emptySet = new HashSet<string>();

        private static readonly ConcurrentDictionary<Type, IReadOnlyList<DataMemberInfo>> TypeDataMemberDic
            = new ConcurrentDictionary<Type, IReadOnlyList<DataMemberInfo>>();

        private static readonly ConcurrentDictionary<Type, Func<object, object, bool>?> TypeEqualsDic
            = new ConcurrentDictionary<Type, Func<object, object, bool>?>();

        internal static IReadOnlyList<DataMemberInfo> GetDataMembers(Type type)
        {
            return TypeDataMemberDic.GetOrAdd(type, GetDataMembersInternal);

            static IReadOnlyList<DataMemberInfo> GetDataMembersInternal(Type type)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(m => m.CanRead);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var members = props
                    .Select(m => new DataMemberInfo(m))
                    .Concat(fields.Select(m => new DataMemberInfo(m)))
                    .OrderBy(m => m.Name)
                    .ToList();
                return members;
            }
        }

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
                    var method = type.GetMethod(nameof(IEquatable<object>.Equals), new[] { type });
                    if (method != null)
                    {
                        return (x, y) => (bool)method.Invoke(x, new[] { y })!;
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

        internal static (bool equal, object? expected, object? actual) Equal(object? value1, object? value2, TreeNode<ExcludeMember>? excludeMemberTree, bool onlyCheckSameNameMembers, HashSet<(object, object)>? visited = null)
        {
            if (value1 == null && value2 == null)
                return (true, null, null);

            if (value1 == null || value2 == null)
                return (false, value1, value2);

            if (ReferenceEquals(value1, value2))
                return (true, value1, value2);

            var (type1, type2) = (value1.GetType(), value2.GetType());

            visited ??= new HashSet<(object, object)>();
            if (IsVisitedType(type1) && IsVisitedType(type2))
                visited.Add((value1, value2));

            Type? typeOfEqual;
            (value1, value2, typeOfEqual) = GetEqualType(type1, value1, type2, value2);

            var equalsMethod = GetEqualsMethod(typeOfEqual);
            if (equalsMethod != null)
                return (equalsMethod(value1, value2), value1, value2);

            if (type1.IsEnumerable() && type2.IsEnumerable())
            {
                using var e1 = ((IEnumerable)value1).GetEnumerator().AsDisposable();
                using var e2 = ((IEnumerable)value2).GetEnumerator().AsDisposable();
                while (true)
                {
                    var b1 = e1.Value.MoveNext();
                    var b2 = e2.Value.MoveNext();

                    if (!b1 && !b2)
                    {
                        return (true, value1, value2);
                    }
                    if (b1 && b2)
                    {
                        var (v1, v2) = (e1.Value.Current, e2.Value.Current);
                        if (v1 == null && v2 == null)
                            continue;

                        if (v1 == null || v2 == null)
                            return (false, value1, value2);

                        if (visited.Contains((v1, v2)))
                            continue;

                        var result = Equal(v1, v2, excludeMemberTree, onlyCheckSameNameMembers, visited);
                        if (!result.equal)
                            return result;
                    }
                    else
                    {
                        return (false, value1, value2);
                    }
                }
            }
            else if (type1 != type2)
            {
                var hasEqual = false;
                var excludeNames = excludeMemberTree?.Children.Where(m => m.Value.IsExcluded).Select(m => m.Value.Name).ToHashSet() ?? _emptySet;
                var members1 = TypeDataMemberDic.GetOrAdd(type1, GetDataMembers).Where(m => !excludeNames.Contains(m.Name)).ToList();
                var members2 = TypeDataMemberDic.GetOrAdd(type2, GetDataMembers).Where(m => !excludeNames.Contains(m.Name)).ToList();

                if (!onlyCheckSameNameMembers && members1.Count != members2.Count)
                    return (false, value1, value2);

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

                    if (visited.Contains((v1, v2)))
                        continue;

                    var result = Equal(v1, v2, exclude, onlyCheckSameNameMembers, visited);
                    if (result.equal)
                        hasEqual = true;
                    else
                        return result;
                }
                return (hasEqual, value1, value2);
            }
            else
            {
                var hasEqual = false;
                var members = TypeDataMemberDic.GetOrAdd(type1, GetDataMembers);
                foreach (var member in members)
                {
                    var exclude = excludeMemberTree?.Children.FirstOrDefault(m => m.Value.Name == member.Name);
                    if (exclude?.Value.IsExcluded == true)
                        continue;

                    var v1 = member.GetValue(value1);
                    var v2 = member.GetValue(value2);

                    if (visited.Contains((v1, v2)))
                        continue;

                    var result = Equal(v1, v2, exclude, onlyCheckSameNameMembers, visited);
                    if (result.equal)
                        hasEqual = true;
                    else
                        return result;
                }
                return (hasEqual, value1, value2);
            }

            static bool IsVisitedType(Type t)
            {
                return !t.IsValueType && t != typeof(string);
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
}
