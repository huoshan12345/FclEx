// ReSharper disable UnassignedField.Global
namespace FclEx.TestModels;

[StructLayout(LayoutKind.Sequential)]
public struct BlittableStruct
{
    public int Int;
    public double Double;

    public override string ToString()
    {
        return $"{nameof(BlittableStruct)}({Int}, {Double}])";
    }
}

[StructLayout(LayoutKind.Sequential)]
public class BlittableClass
{
    public int Int;
    public double Double;

    public override string ToString()
    {
        return $"{nameof(BlittableClass)}({Int}, {Double}])";
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct MarshalableStruct
{
    public int Int;
    public char Char;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[]? Array;

    public override string ToString()
    {
        return $"{nameof(MarshalableStruct)}({Int}, {Char.ToLiteral()}, [{Array?.JoinWith(", ")}])";
    }
}

[StructLayout(LayoutKind.Sequential)]
public class MarshalableClass
{
    public int Int;
    public char Char;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[]? Array;

    public override string ToString()
    {
        return $"{nameof(MarshalableClass)}({Int}, {Char.ToLiteral()}, [{Array?.JoinWith(", ")}])";
    }
}