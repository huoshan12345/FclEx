// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ConvertToConstant.Local

using static FclEx.Utils.SizeCalculator;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
namespace FclEx.Utils;

public unsafe class SizeCalculatorTests
{
    class EmptyClass { }

    struct MyStruct
    {
        public int a;
        public double b;
    }

    struct AlignedStruct
    {
        public int a;
        public double b;
        public int c;
        public char d;
    }

    struct AlignedStruct2
    {
        public char d;
        public int a;
        public int c;
        public double b;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct AlignedStructPack4
    {
        public char d;
        public int a;
        public int c;
        public double b;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct AlignedStructPack8
    {
        public char d;
        public int a;
        public int c;
        public double b;
    }

    class MyClass
    {
        public int a;
        public double b;
    }

    struct NestedStruct
    {
        public int x;
        public double y;
    }

    struct OuterStruct
    {
        public NestedStruct nested;
        public int z;
    }

    class NestedClass
    {
        public int x;
        public double y;
    }

    class OuterClass
    {
        public NestedClass nested;
        public int z;
    }

    class OuterClassContainsStruct
    {
        public NestedClass nested;
        public int z;
        public NestedStruct nested2;
    }

    class MyReferenceType
    {
        public int a;
    }

    class RecursiveClass
    {
        public RecursiveClass? next;
        public int value;
    }

    struct StructContainsArray
    {
        public int[] x;
        public double y;
    }

    class ClassContainsArray
    {
        public int[] x;
        public double y;
    }

    [Fact]
    public void SizeOf_Int()
    {
        var expectedSize = sizeof(int);
        var actualSize = SizeOf<int>();
        Assert.Equal(expectedSize, actualSize);
    }

    [Fact]
    public void SizeOf_Double()
    {
        var expectedSize = sizeof(double);
        var actualSize = SizeOf<double>();
        Assert.Equal(expectedSize, actualSize);
    }

    [Fact]
    public void SizeOf_Struct()
    {
        var expectedSize = sizeof(MyStruct);
        var actualSize = SizeOf<MyStruct>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }

    [Fact]
    public void SizeOf_Class()
    {
        var expectedSize = sizeof(int) + sizeof(double) + IntPtr.Size * 2;
        var actualSize = SizeOf<MyClass>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }

    [Fact]
    public void SizeOf_Array()
    {
        var expectedSize = IntPtr.Size * 3;
        var actualSize = SizeOf<int[]>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }

    [Fact]
    public void SizeOf_EmptyClass()
    {
        var expectedSize = IntPtr.Size * 3;
        var actualSize = SizeOf<EmptyClass>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }

    [Fact]
    public void SizeOf_NestedStruct()
    {
        var expectedSize = sizeof(OuterStruct);
        var actualSize = SizeOf<OuterStruct>();
        Assert.Equal(expectedSize, actualSize);
    }

    [Fact]
    public void SizeOf_NestedClass()
    {
        {
            var expectedSize = IntPtr.Size * 2 + sizeof(int) + IntPtr.Size;
            var actualSize = SizeOf<OuterClass>();
            Assert.Equal(RoundUpSize(expectedSize), actualSize);
        }
        {
            var expectedSize = IntPtr.Size * 2 + sizeof(int) + IntPtr.Size + SizeOf<NestedStruct>();
            var actualSize = SizeOf<OuterClassContainsStruct>();
            Assert.Equal(RoundUpSize(expectedSize), actualSize);
        }
    }

    [Fact]
    public void SizeOf_AlignedStruct()
    {
        {
            var expectedSize = sizeof(AlignedStruct);
            var actualSize = SizeOf<AlignedStruct>();
            Assert.Equal(expectedSize, actualSize);
        }
        {
            var expectedSize = sizeof(AlignedStruct2);
            var actualSize = SizeOf<AlignedStruct2>();
            Assert.Equal(expectedSize, actualSize);
        }
        {
            var size = sizeof(AlignedStructPack4);
            var expectedSize = NET60_OR_GREATER.IsMatch()
                ? size
                : RoundUpSize(size);

            var actualSize = SizeOf<AlignedStructPack4>();
            Assert.Equal(expectedSize, actualSize);
        }
        {
            var expectedSize = sizeof(AlignedStructPack8);
            var actualSize = SizeOf<AlignedStructPack8>();
            Assert.Equal(expectedSize, actualSize);
        }
    }

    [Fact]
    public void SizeOf_ArrayOfReferenceType()
    {
        var expectedSize = IntPtr.Size * 3;
        var actualSize = SizeOf<MyReferenceType[]>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }

    [Fact]
    public void SizeOf_RecursiveClass()
    {
        var expectedSize = IntPtr.Size * 2 + IntPtr.Size + sizeof(int);
        var actualSize = SizeOf<RecursiveClass>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }


    [Fact]
    public void SizeOf_StructContainsArray()
    {
        var expectedSize = IntPtr.Size + sizeof(double);
        var actualSize = SizeOf<StructContainsArray>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }

    [Fact]
    public void SizeOf_ClassContainsArray()
    {
        var expectedSize = IntPtr.Size * 2 + IntPtr.Size + sizeof(double);
        var actualSize = SizeOf<ClassContainsArray>();
        Assert.Equal(RoundUpSize(expectedSize), actualSize);
    }

    internal static int RoundUpSize(int size)
    {
        return RoundUp(size, IntPtr.Size);
    }

    internal static int RoundUp(int size, int @base)
    {
        Check.NotLessThan(size, 0);
        Check.GreaterThan(@base, 0);

        var remaining = size % @base;
        return remaining == 0
            ? size
            : size + (@base - remaining);
    }
}