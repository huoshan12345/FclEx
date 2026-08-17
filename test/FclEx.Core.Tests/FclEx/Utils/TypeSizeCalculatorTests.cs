// ReSharper disable ClassNeverInstantiated.Local
#pragma warning disable CS0649
#pragma warning disable CS0169

namespace FclEx.Utils;

public class TypeSizeCalculatorTests
{
    private sealed class EmptyClass;

    private struct SampleStruct
    {
        public int Number;
        public double Amount;
    }

    private sealed class ReferencedClass;

    private class BaseClass
    {
        private long _baseValue;
    }

    private sealed class SampleClass : BaseClass
    {
        private int _number;
        private SampleStruct _value;
        private ReferencedClass? _reference;
    }

    private abstract class AbstractClass
    {
        private int _value;
    }

    [Fact]
    public void GetInstanceFieldStorageSize_ValueType_ReturnsInlineManagedSize()
    {
        Assert.Equal(UnsafeHelper.SizeOf<SampleStruct>(), TypeSizeCalculator.GetInstanceFieldStorageSize<SampleStruct>());
    }

    [Fact]
    public void GetInstanceFieldStorageSize_ReferenceType_SumsInheritedAndDeclaredFields()
    {
        var expected = sizeof(long) + sizeof(int) + UnsafeHelper.SizeOf<SampleStruct>() + IntPtr.Size;

        Assert.Equal(expected, TypeSizeCalculator.GetInstanceFieldStorageSize<SampleClass>());
    }

    [Fact]
    public void GetInstanceFieldStorageSize_EmptyClass_ReturnsZero()
    {
        Assert.Equal(0, TypeSizeCalculator.GetInstanceFieldStorageSize<EmptyClass>());
    }

    [Fact]
    public void GetInstanceFieldStorageSize_String_SumsItsDeclaredFields()
    {
        var expected = typeof(string).GetAllInstanceFields().Sum(field =>
            field.FieldType.IsValueType ? UnsafeHelper.SizeOf(field.FieldType) : IntPtr.Size);

        Assert.Equal(expected, TypeSizeCalculator.GetInstanceFieldStorageSize<string>());
    }

    [Theory]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(AbstractClass))]
    [InlineData(typeof(IEnumerable<>))]
    [InlineData(typeof(List<>))]
    public void GetInstanceFieldStorageSize_UnsupportedType_ThrowsArgumentException(Type type)
    {
        Assert.Throws<ArgumentException>(() => TypeSizeCalculator.GetInstanceFieldStorageSize(type));
    }
}
