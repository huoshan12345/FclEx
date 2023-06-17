using System;
using System.Linq;

namespace FclEx.Extensions;

public class ExpressionExtensionsTests
{
    [Fact]
    public void Or_BothNull_Throw()
    {
        Expression<Func<object, bool>>? left = null;
        Assert.Throws<ArgumentNullException>(() => left.Or(null));
    }

    [Fact]
    public void Or_LeftNull_ReturnRight()
    {
        Expression<Func<object, bool>>? left = null;
        Expression<Func<object, bool>> right = m => true;
        var merge = left.Or(right);
        Assert.Same(right, merge);
    }

    [Fact]
    public void Or_RightNull_ReturnLeft()
    {
        Expression<Func<object, bool>> left = m => true;
        Expression<Func<object, bool>>? right = null;
        var merge = left.Or(right);
        Assert.Same(left, merge);
    }

    [Fact]
    public void Or_BothNotNull_ReturnMerge()
    {
        Expression<Func<int, bool>> left = m => m < 3;
        Expression<Func<int, bool>> right = m => m > 8;
        var merge = left.Or(right);
        var list = Enumerable.Range(1, 10).ToList();
        var expected = list.Where(m => m < 3 || m > 8).ToList();
        var actual = list.Where(merge.Compile()).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void And_BothNull_Throw()
    {
        Expression<Func<object, bool>>? left = null;
        Assert.Throws<ArgumentNullException>(() => left.And(null));
    }

    [Fact]
    public void And_LeftNull_ReturnRight()
    {
        Expression<Func<object, bool>>? left = null;
        Expression<Func<object, bool>> right = m => true;
        var merge = left.And(right);
        Assert.Same(right, merge);
    }

    [Fact]
    public void And_RightNull_ReturnLeft()
    {
        Expression<Func<object, bool>> left = m => true;
        Expression<Func<object, bool>>? right = null;
        var merge = left.And(right);
        Assert.Same(left, merge);
    }

    [Fact]
    public void And_BothNotNull_ReturnMerge()
    {
        Expression<Func<int, bool>> left = m => m > 3;
        Expression<Func<int, bool>> right = m => m < 8;
        var merge = left.And(right);
        var list = Enumerable.Range(1, 10).ToList();
        var expected = list.Where(m => m > 3 && m < 8).ToList();
        var actual = list.Where(merge.Compile()).ToList();
        Assert.Equal(expected, actual);
    }
}