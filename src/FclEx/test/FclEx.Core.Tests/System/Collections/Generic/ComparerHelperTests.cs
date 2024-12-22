namespace System.Collections.Generic;

public class ComparerHelperTests
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

    [Fact]
    public void TryCompare_BothNull_ReturnsTrueWithResultZero()
    {
        Assert.True(ComparerHelper.TryCompare<string>(null, null, out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryCompare_XNull_ReturnsTrueWithResultNegativeOne()
    {
        Assert.True(ComparerHelper.TryCompare<string>(null, "test", out var result));
        Assert.Equal(-1, result);
    }

    [Fact]
    public void TryCompare_YNull_ReturnsTrueWithResultOne()
    {
        Assert.True(ComparerHelper.TryCompare<string>("test", null, out var result));
        Assert.Equal(1, result);
    }

    [Fact]
    public void TryCompare_ReferenceEquals_ReturnsTrueWithResultZero()
    {
        var obj = new object();
        Assert.True(ComparerHelper.TryCompare(obj, obj, out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryCompare_CustomTypeWithOverloadedEqualityOperator_UsesReferenceEquals()
    {
        var obj1 = new TestModel(1);
        var obj2 = new TestModel(1);
        Assert.False(ReferenceEquals(obj1, obj2));
        Assert.True(ComparerHelper.TryCompare(obj1, obj1, out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryEquals_BothNull_ReturnsTrueWithResultTrue()
    {
        Assert.True(ComparerHelper.TryEquals<string>(null, null, out var result));
        Assert.True(result);
    }

    [Fact]
    public void TryEquals_XNull_ReturnsTrueWithResultFalse()
    {
        Assert.True(ComparerHelper.TryEquals<string>(null, "test", out var result));
        Assert.False(result);
    }

    [Fact]
    public void TryEquals_YNull_ReturnsTrueWithResultFalse()
    {
        Assert.True(ComparerHelper.TryEquals<string>("test", null, out var result));
        Assert.False(result);
    }

    [Fact]
    public void TryEquals_ReferenceEquals_ReturnsTrueWithResultTrue()
    {
        var obj = new object();
        Assert.True(ComparerHelper.TryEquals(obj, obj, out var result));
        Assert.True(result);
    }

    [Fact]
    public void TryEquals_CustomTypeWithOverloadedEqualityOperator_UsesReferenceEquals()
    {
        var obj1 = new TestModel(1);
        var obj2 = new TestModel(1);
        Assert.False(ReferenceEquals(obj1, obj2));
        Assert.False(ComparerHelper.TryEquals(obj1, obj2, out _));
    }
}