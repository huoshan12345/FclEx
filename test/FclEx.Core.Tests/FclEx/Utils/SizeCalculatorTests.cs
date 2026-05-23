// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ConvertToConstant.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using static FclEx.Utils.SizeCalculator;

namespace FclEx.Utils;

public unsafe class SizeCalculatorTests
{
    class EmptyClass;

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
            var actualSize = SizeOf<AlignedStructPack4>();
            Assert.Equal(size, actualSize);
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

    [Theory]
    [InlineData(typeof(ValueTuple<int>))]
    [InlineData(typeof(ValueTuple<int, long>))]
    public void SizeOf_ValueTuple_Test(Type type)
    {
        var expectedSize = UnsafeHelper.SizeOf(type);
        var actualSize = SizeOf(type);
        Assert.Equal(expectedSize, actualSize);
    }

    [Theory]
    [InlineData(typeof(Action))]
    [InlineData(typeof(Func<int>))]
    public void SizeOf_DelegateType_Test(Type type)
    {
        /*
         * object? _target;
         * object? _methodBase;
         * IntPtr _methodPtr;
         * IntPtr _methodPtrAux;
         * object? _invocationList;
         * nint _invocationCount;
         */
        var expectedSize = sizeof(object) * 3 + sizeof(IntPtr) * 3 + IntPtr.Size * 2;
        var actualSize = SizeOf(type);
        Assert.Equal(expectedSize, actualSize);
    }

    [Theory]
    [InlineData(typeof(TextWriter))]
    public void SizeOf_AbstractType_Test(Type type)
    {
        var ex = Assert.Throws<MemberAccessException>(() => SizeOf(type));
        Assert.Contains("Cannot create an abstract class.", ex.Message);
    }

    [Theory]
    [InlineData(typeof(List<>))]
    public void SizeOf_OpenGenericType_Test(Type type)
    {
        var ex = Assert.Throws<MemberAccessException>(() => SizeOf(type));
        Assert.Contains("Cannot create a type for which Type.ContainsGenericParameters is true.", ex.Message);
    }

    [Theory]
    [InlineData(typeof(IEnumerable))]
    [InlineData(typeof(IEnumerable<>))]
    public void SizeOf_Interface_Test(Type type)
    {
        var actualSize = SizeOf(type);
        Assert.Equal(IntPtr.Size * 3, actualSize);
    }

    internal static int RoundUpSize(int size)
    {
        return size.RoundUpTo(IntPtr.Size);
    }
}