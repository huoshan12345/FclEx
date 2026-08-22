namespace FclEx.Extensions;

public class ComparerExtensionsTests
{
    public class TestModel(int value)
    {
        public int Value { get; } = value;

        public static bool operator ==(TestModel? left, TestModel? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Value == right.Value;
        }

        public static bool operator !=(TestModel? left, TestModel? right) => !(left == right);

        public override bool Equals(object? obj)
        {
            return obj is TestModel other && this == other;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    private interface IValue;

    private sealed class FirstValue : IValue;

    private sealed class SecondValue : IValue;

    [Fact]
    public void TryCompare_BothNull_ReturnsTrueWithResultZero()
    {
        Assert.True(Comparer.TryCompare<string>(null, null, out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryCompare_XNull_ReturnsTrueWithResultNegativeOne()
    {
        Assert.True(Comparer.TryCompare<string>(null, "test", out var result));
        Assert.Equal(-1, result);
    }

    [Fact]
    public void TryCompare_YNull_ReturnsTrueWithResultOne()
    {
        Assert.True(Comparer.TryCompare<string>("test", null, out var result));
        Assert.Equal(1, result);
    }

    [Fact]
    public void TryCompare_ReferenceEquals_ReturnsTrueWithResultZero()
    {
        var obj = new object();
        Assert.True(Comparer.TryCompare(obj, obj, out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryCompare_CustomTypeWithOverloadedEqualityOperator_UsesReferenceEquals()
    {
        var obj1 = new TestModel(1);
        var obj2 = new TestModel(1);
        Assert.False(ReferenceEquals(obj1, obj2));
        Assert.True(Comparer.TryCompare(obj1, obj1, out var result));
        Assert.Equal(0, result);

        Assert.False(Comparer.TryCompare(obj1, obj2, out result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void TryCompare_NonNullableValueTypes_DefersValueComparison(int x, int y)
    {
        Assert.False(Comparer.TryCompare(x, y, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void TryCompare_NullableValueTypes_BothNull_ReturnsTrueWithResultZero()
    {
        Assert.True(Comparer.TryCompare<int?>(null, null, out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryCompare_NullableValueTypes_XNull_ReturnsTrueWithResultNegativeOne()
    {
        Assert.True(Comparer.TryCompare<int?>(null, 0, out var result));
        Assert.Equal(-1, result);
    }

    [Fact]
    public void TryCompare_NullableValueTypes_YNull_ReturnsTrueWithResultOne()
    {
        Assert.True(Comparer.TryCompare<int?>(0, null, out var result));
        Assert.Equal(1, result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void TryCompare_NullableValueTypes_WithValues_DefersValueComparison(int? x, int? y)
    {
        Assert.False(Comparer.TryCompare(x, y, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void TryEquals_BothNull_ReturnsTrueWithResultTrue()
    {
        Assert.True(Comparer.TryEquals<string>(null, null, out var result));
        Assert.True(result);
    }

    [Fact]
    public void TryEquals_XNull_ReturnsTrueWithResultFalse()
    {
        Assert.True(Comparer.TryEquals<string>(null, "test", out var result));
        Assert.False(result);
    }

    [Fact]
    public void TryEquals_YNull_ReturnsTrueWithResultFalse()
    {
        Assert.True(Comparer.TryEquals<string>("test", null, out var result));
        Assert.False(result);
    }

    [Fact]
    public void TryEquals_ReferenceEquals_ReturnsTrueWithResultTrue()
    {
        var obj = new object();
        Assert.True(Comparer.TryEquals(obj, obj, out var result));
        Assert.True(result);
    }

    [Fact]
    public void TryEquals_CustomTypeWithOverloadedEqualityOperator_UsesReferenceEquals()
    {
        var obj1 = new TestModel(1);
        var obj2 = new TestModel(1);
        Assert.False(ReferenceEquals(obj1, obj2));
        Assert.False(Comparer.TryEquals(obj1, obj2, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void TryEquals_DifferentRuntimeTypes_Can_Be_Left_For_The_Caller()
    {
        IValue x = new FirstValue();
        IValue y = new SecondValue();

        Assert.True(Comparer.TryEquals(x, y, out var strictResult));
        Assert.False(strictResult);

        Assert.False(Comparer.TryEquals(x, y, out var deferredResult, requireSameRuntimeType: false));
        Assert.Null(deferredResult);
    }

    [Fact]
    public void TryEquals_SameRuntimeType_WithRuntimeTypeCheckDisabled_DefersValueComparison()
    {
        IValue x = new FirstValue();
        IValue y = new FirstValue();

        Assert.False(Comparer.TryEquals(x, y, out var result, requireSameRuntimeType: false));
        Assert.Null(result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void TryEquals_NonNullableValueTypes_DefersValueComparison(int x, int y)
    {
        Assert.False(Comparer.TryEquals(x, y, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void TryEquals_NullableValueTypes_BothNull_ReturnsTrueWithResultTrue()
    {
        Assert.True(Comparer.TryEquals<int?>(null, null, out var result));
        Assert.True(result);
    }

    [Fact]
    public void TryEquals_NullableValueTypes_XNull_ReturnsTrueWithResultFalse()
    {
        Assert.True(Comparer.TryEquals<int?>(null, 0, out var result));
        Assert.False(result);
    }

    [Fact]
    public void TryEquals_NullableValueTypes_YNull_ReturnsTrueWithResultFalse()
    {
        Assert.True(Comparer.TryEquals<int?>(0, null, out var result));
        Assert.False(result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void TryEquals_NullableValueTypes_WithValues_DefersValueComparison(int? x, int? y)
    {
        Assert.False(Comparer.TryEquals(x, y, out var result));
        Assert.Null(result);
    }
}
