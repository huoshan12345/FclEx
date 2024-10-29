// ReSharper disable UnassignedField.Global
namespace FclEx.TestModels;

[StructLayout(LayoutKind.Sequential)]
public struct BlittableStruct
{
    public int Number;
    public char Char;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Arr;

    public override string ToString()
    {
        return $"{nameof(BlittableStruct)}({Number}, {(ushort)Char}, [{Arr?.JoinWith(", ")}])";
    }
}

[StructLayout(LayoutKind.Sequential)]
public class BlittableClass
{
    public int Number;
    public char Char;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[]? Arr;

    public override string ToString()
    {
        return $"{nameof(BlittableClass)}({Number}, {(ushort)Char}, [{Arr?.JoinWith(", ")}])";
    }
}