using System.Numerics;
using System.Runtime.InteropServices;
using FclEx.Comparers;

namespace FclEx.TestModels;

[StructLayout(LayoutKind.Sequential)]
public struct UnmanagedStruct : IEquatable<UnmanagedStruct>
#if NET7_0_OR_GREATER
  , IEqualityOperators<UnmanagedStruct, UnmanagedStruct, bool>
#endif
{
    private static readonly IEqualityComparer<UnmanagedStruct> _comparer = BytewiseEqualityComparer<UnmanagedStruct>.Instance;

    public int Number;
    public char Char;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Arr;

    public bool Equals(UnmanagedStruct other)
    {
        return _comparer.Equals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is UnmanagedStruct other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _comparer.GetHashCode(this);
    }

    public static bool operator ==(UnmanagedStruct left, UnmanagedStruct right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UnmanagedStruct left, UnmanagedStruct right)
    {
        return left == right == false;
    }

    public override string ToString()
    {
        return $"{nameof(UnmanagedStruct)}({Number}, {Char}, [{Arr.JoinWith(", ")}])";
    }
}