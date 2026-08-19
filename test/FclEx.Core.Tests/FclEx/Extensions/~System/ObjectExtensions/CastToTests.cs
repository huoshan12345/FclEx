namespace FclEx.Extensions.ObjectExtensions;

[SuppressMessage("ReSharper", "MergeConditionalExpression")]
public class CastToTests
{
    private sealed class ImplicitSource(int value)
    {
        public int Value { get; } = value;

        public static implicit operator ImplicitTarget(ImplicitSource value) => new(value.Value);
    }

    private sealed class ImplicitTarget(int value)
    {
        public int Value { get; } = value;
    }

    private sealed class ExplicitSource(int value)
    {
        public int Value { get; } = value;
    }

    private sealed class ExplicitTarget(int value)
    {
        public int Value { get; } = value;

        public static explicit operator ExplicitTarget(ExplicitSource value) => new(value.Value);
    }

    private class BaseSource(int value)
    {
        public int Value { get; } = value;
    }

    private sealed class DerivedSource(int value) : BaseSource(value);

    private sealed class BaseParameterTarget(int value)
    {
        public int Value { get; } = value;

        public static implicit operator BaseParameterTarget(BaseSource value) => new(value.Value);
    }

    public enum ShortEnum : short
    {
        No = 0,
        Yes = 1,
    }
    public enum IntEnum : int
    {
        No = 0,
        Yes = 1,
    }

    [Theory]
    [InlineData(5)]
    [InlineData(null)]
    public void ObjectToIntCastTest(object? obj)
    {
        var actual = obj.CastTo<int>();
        var expected = obj == null ? default : (int)obj;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IntToObjectCastTest()
    {
        var i = 5;
        var actual = i.CastTo<object>();
        var expected = (object)i;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DoubleToIntCastTest()
    {
        var i = 5.0;
        var actual = i.CastTo<int>();
        var expected = (int)i;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IntToDoubleCastTest()
    {
        var i = 5;
        var actual = i.CastTo<double>();
        var expected = (double)i;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void StringToIntCastTest()
    {
        Assert.Equal(42, "42".CastTo<int>());
    }

    [Fact]
    public void CastTo_UsesAnImplicitOperatorDeclaredOnTheSourceType()
    {
        var result = new ImplicitSource(42).CastTo<ImplicitTarget>();

        Assert.Equal(42, result!.Value);
    }

    [Fact]
    public void CastTo_UsesAnExplicitOperatorDeclaredOnTheTargetType()
    {
        var result = new ExplicitSource(42).CastTo<ExplicitTarget>();

        Assert.Equal(42, result!.Value);
    }

    [Fact]
    public void CastTo_UsesAnOperatorWhoseParameterIsTheSourceBaseType()
    {
        var result = new DerivedSource(42).CastTo<BaseParameterTarget>();

        Assert.Equal(42, result!.Value);
    }

    [Fact]
    public void Clamp_RejectsAnInvertedRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 10.Clamp(5, 1));
    }

    [Fact]
    public void IntToEnumCastTest()
    {
        var i = 1;
        var actual = i.CastTo<IntEnum>();
        var expected = (IntEnum)i;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IntToShortEnumCastTest()
    {
        var i = 1;
        var actual = i.CastTo<ShortEnum>();
        var expected = (ShortEnum)i;
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(IntEnum.Yes)]
    [InlineData(null)]
    public void EnumToIntCastTest(IntEnum? i)
    {
        var actual = i.CastTo<int>();
        var expected = (int)i.Get();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ShortEnumToIntCastTest()
    {
        var i = ShortEnum.Yes;
        var actual = i.CastTo<int>();
        var expected = (int)i;
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public void IntToNullableCastTest(int i)
    {
        var actual = i.CastTo<int?>();
        var expected = (int?)i;
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(null)]
    public void NullableToIntCastTest(int? i)
    {
        var actual = i.CastTo<int>();
        var expected = i.Get();
        Assert.Equal(expected, actual);
    }
}
