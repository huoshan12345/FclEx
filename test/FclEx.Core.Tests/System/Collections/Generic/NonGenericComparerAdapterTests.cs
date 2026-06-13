namespace System.Collections.Generic;

public class NonGenericComparerAdapterTests
{
    [Fact]
    public void Compare_ShouldUseFallbackComparer_WhenComparerIsNotProvided()
    {
        var comparer = new NonGenericComparerAdapter<int>();

        Assert.True(comparer.Compare(1, 2) < 0);
        Assert.True(comparer.Compare(2, 1) > 0);
        Assert.Equal(0, comparer.Compare(1, 1));
    }

    [Fact]
    public void Compare_ShouldHandleNullOperands_BeforeCasting()
    {
        var comparer = new NonGenericComparerAdapter<int>();

        Assert.True(comparer.Compare(null, 1) < 0);
        Assert.True(comparer.Compare(1, null) > 0);
        Assert.Equal(0, comparer.Compare(null, null));
    }

    [Fact]
    public void Compare_ShouldThrowInvalidCastException_ForWrongOperandType()
    {
        var comparer = new NonGenericComparerAdapter<int>();

        Assert.Throws<InvalidCastException>(() => comparer.Compare("1", 1));
    }
}
