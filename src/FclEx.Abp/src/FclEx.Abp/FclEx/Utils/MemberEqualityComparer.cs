using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FclEx.Extensions;

namespace FclEx.Utils;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public class MemberEqualityComparer<T> : IEqualityComparer<T>
{
    public MemberEqualityComparer(IEnumerable<string> excludeMemberNames)
    {
        ExcludeMemberNames = excludeMemberNames.Touch().NotNull().ToHashSet();
    }

    public HashSet<string> ExcludeMemberNames { get; }

    public static IList<DataMemberInfo> ReadableMembers { get; } = GetReadableMembers();

    private static IList<DataMemberInfo> GetReadableMembers()
    {
        var type = typeof(T);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(m => m.CanRead);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var members = props
            .Select(m => new DataMemberInfo(m))
            .Concat(fields.Select(m => new DataMemberInfo(m)))
            .ToList();
        return members;
    }

    private static ConcurrentDictionary<Type, IEqualityComparer> Comparers { get; } = new();

    public bool Equals(T? x, T? y)
    {
        foreach (var member in ReadableMembers.Where(m => !ExcludeMemberNames.Contains(m.Name)))
        {
            var v1 = member.GetValue(x);
            var v2 = member.GetValue(y);
            var cmp = Comparers.GetOrAdd(member.DeclaringType!, key =>
            {
                var comparer = typeof(EqualityComparer<>).MakeGenericType(key).InvokeMember(
                    nameof(EqualityComparer<object>.Default),
                    BindingFlags.Static | BindingFlags.Public, null, null, null);
                return (IEqualityComparer)comparer!;
            });
            if (!cmp.Equals(v1, v2))
                return false;
        }
        return true;
    }

    public int GetHashCode(T obj)
    {
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            foreach (var member in ReadableMembers.Where(m => !ExcludeMemberNames.Contains(m.Name)))
            {
                var value = member.GetValue(obj);
                hash = hash * 23 + value.GetHashCodeSafely();
            }
            return hash;
        }
    }
}