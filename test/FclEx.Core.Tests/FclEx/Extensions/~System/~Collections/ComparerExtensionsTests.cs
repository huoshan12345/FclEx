namespace FclEx.Extensions;

public class ComparerExtensions
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
        Assert.False(Comparer.TryEquals(obj1, obj2, out _));
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
}
