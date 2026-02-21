namespace System.Reflection;

public class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
{
    public static MemberInfoEqualityComparer Instance { get; } = new();

    public bool Equals(MemberInfo? x, MemberInfo? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        return (x, y) switch
        {
            (MethodInfo { DeclaringType.IsArray: true } mx, MethodInfo { DeclaringType.IsArray: true } my) 
                => mx.MethodHandle == my.MethodHandle,
            _ => x.Module == y.Module && x.MetadataToken == y.MetadataToken
        };
    }

    public int GetHashCode(MemberInfo obj)
    {
        return obj switch
        {
            MethodInfo { DeclaringType.IsArray: true } m => m.MethodHandle.GetHashCode(),
            _ => HashCode.Combine(obj.Module, obj.MetadataToken)
        };
    }
}

