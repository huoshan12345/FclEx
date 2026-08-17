using FclEx.TestModels;
using Xunit.Sdk;

namespace System.Collections.Generic;

public class MarshalToBytesEqualityComparerTests
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct PointerMarshaledString
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string? Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaddedValue
    {
        public byte First;
        public int Second;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct InlineString
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string? Value;
    }

    public static readonly IEnumerable<Type> ValueTypes = Types.BlittableTypes.Concat([
        typeof(decimal),
        typeof(TimeSpan),
        typeof(Guid),
#if NET8_0_OR_GREATER
        typeof(DateOnly),
        typeof(TimeOnly),  
#endif
        typeof(MarshalableClass),
        typeof(MarshalableStruct)]);

    public static readonly TheoryData<Type> TypeCases = ValueTypes.ToTheoryData();

    private static readonly MethodInfo _equals = typeof(MarshalToBytesEqualityComparerTests).GetRequiredMethod(nameof(Equals));

    [Theory]
    [MemberData(nameof(TypeCases))]
    public void Equals_Test(Type type)
    {
        _equals.MakeGenericMethod(type).Invoke(this, null);
    }

    private void Equals<T>()
    {
        var random = new Random(0);
        var x = random.Next<T>();
        Assert.Equal<T>(x, x, MarshalToBytesEqualityComparer<T>.Instance);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(CommonRecord))]
    [InlineData(typeof(CommonStruct))]
    [InlineData(typeof(CommonRecordStruct))]
    public void Equals_AutoLayout_Test(Type type)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => _equals.MakeGenericMethod(type).Invoke(this, null));
        var inner = Assert.IsType<EqualException>(ex.InnerException);
        var innermost = Assert.IsType<ArgumentException>(inner.InnerException);
        Assert.Contains("is not marshalable because it is auto layout.", innermost.Message);
    }

    [Theory]
    [InlineData(typeof(Tuple<int>))]
    [InlineData(typeof(ValueTuple<int>))]
    public void Equals_Generic_Test(Type type)
    {
        var ex = Assert.Throws<TargetInvocationException>(() => _equals.MakeGenericMethod(type).Invoke(this, null));
        var inner = Assert.IsType<EqualException>(ex.InnerException);
        var innermost = Assert.IsType<ArgumentException>(inner.InnerException);
        Assert.Contains("is not marshalable because it is generic", innermost.Message);
    }

    [Fact]
    public void Equal_Values_With_Padding_Should_Produce_Stable_Comparison_And_Hash()
    {
        var comparer = MarshalToBytesEqualityComparer<PaddedValue>.Instance;
        var x = new PaddedValue { First = 1, Second = 2 };
        var y = new PaddedValue { First = 1, Second = 2 };

        for (var i = 0; i < 100; i++)
        {
            Assert.True(comparer.Equals(x, y));
            Assert.Equal(comparer.GetHashCode(x), comparer.GetHashCode(y));
        }
    }

    [Fact]
    public void MarshalToBytes_ClearNativeBuffer_Should_Clear_Padding()
    {
        var value = new PaddedValue { First = 1, Second = 2 };
        var bytes = Marshal.ToBytes(value, clearNativeBuffer: true);
        var secondOffset = Marshal.OffsetOf<PaddedValue>(nameof(PaddedValue.Second)).ToInt32();

        Assert.True(secondOffset > 1);
        for (var i = 1; i < secondOffset; i++)
        {
            Assert.Equal(0, bytes[i]);
        }
    }

    [Fact]
    public void Equivalent_Inline_Array_Values_Should_Compare_Equal()
    {
        var comparer = MarshalToBytesEqualityComparer<MarshalableStruct>.Instance;
        var x = new MarshalableStruct { Int = 1, Char = 'x', Array = [1, 2, 3, 4] };
        var y = new MarshalableStruct { Int = 1, Char = 'x', Array = [1, 2, 3, 4] };

        Assert.True(comparer.Equals(x, y));
        Assert.Equal(comparer.GetHashCode(x), comparer.GetHashCode(y));
    }

    [Fact]
    public void Equivalent_Inline_String_Values_Should_Compare_Equal()
    {
        var comparer = MarshalToBytesEqualityComparer<InlineString>.Instance;
        var x = new InlineString { Value = new string('x', 4) };
        var y = new InlineString { Value = new string('x', 4) };

        Assert.True(comparer.Equals(x, y));
        Assert.Equal(comparer.GetHashCode(x), comparer.GetHashCode(y));
    }

    [Fact(Skip = "Pointer-based marshaling can reuse a freed native address, so two independent values can produce equal raw marshaled bytes; see issue 114.")]
    public void Different_Pointer_Based_Marshal_Fields_Should_Not_Compare_Equal()
    {
        var comparer = MarshalToBytesEqualityComparer<PointerMarshaledString>.Instance;
        var x = new PointerMarshaledString { Value = new string('x', 4) };
        var y = new PointerMarshaledString { Value = new string('x', 4) };

        Assert.False(comparer.Equals(x, y));
    }

    [Fact(Skip = "Pointer-based marshaling creates a new native address on each call, so raw marshaled bytes cannot provide a reflexive equality relation; see issue 114.")]
    public void Same_Value_With_Pointer_Based_Marshal_Field_Should_Compare_Equal()
    {
        var comparer = MarshalToBytesEqualityComparer<PointerMarshaledString>.Instance;
        var value = new PointerMarshaledString { Value = new string('x', 4) };

        Assert.True(comparer.Equals(value, value));
    }

}
