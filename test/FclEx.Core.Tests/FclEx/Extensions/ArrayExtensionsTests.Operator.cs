namespace FclEx.Extensions;

partial class ArrayExtensionsTests
{
    [Fact]
    public void Plus_Basic()
    {
        int[] a = [1, 2];
        int[] b = [3, 4];
        var result = a + b;

        Assert.Equal([1, 2, 3, 4], result);
    }

    [Fact]
    public void Plus_WithEmpty()
    {
        int[] a = [1, 2];
        int[] b = [];
        var result = a + b;

        Assert.Equal([1, 2], result);
    }

    [Fact]
    public void Plus_EmptyWithArray()
    {
        int[] a = [];
        int[] b = [3, 4];
        var result = a + b;

        Assert.Equal([3, 4], result);
    }

    [Fact]
    public void Plus_SelfConcat()
    {
        int[] a = [1, 2];
        var result = a + a;

        Assert.Equal([1, 2, 1, 2], result);
    }

    [Fact]
    public void Plus_StringArray()
    {
        string[] a = ["a", "b"];
        string[] b = ["c"];
        var result = a + b;

        Assert.Equal(["a", "b", "c"], result);
    }

    [Fact]
    public void PlusEquals_Basic()
    {
        int[] a = [1, 2];
        int[] b = [3, 4];
        a += b;

        Assert.Equal([1, 2, 3, 4], a);
    }

    [Fact]
    public void PlusEquals_WithEmpty()
    {
        int[] a = [1, 2];
        int[] b = [];
        a += b;

        Assert.Equal([1, 2], a);
    }

    [Fact]
    public void PlusEquals_EmptyArray()
    {
        int[] a = [];
        int[] b = [3, 4];
        a += b;

        Assert.Equal([3, 4], a);
    }

    [Fact]
    public void PlusEquals_MultipleTimes()
    {
        int[] a = [1];
        int[] b = [2];
        int[] c = [3];
        a += b;
        a += c;

        Assert.Equal([1, 2, 3], a);
    }

    [Fact]
    public void PlusEquals_SelfConcat()
    {
        int[] a = [1, 2];
        a += a;

        Assert.Equal([1, 2, 1, 2], a);
    }

    [Fact]
    public void PlusEquals_ResultLengthCorrect()
    {
        int[] a = [1, 2];
        int[] b = [3, 4, 5];
        a += b;

        Assert.Equal(5, a.Length);
    }

    [Fact]
    public void Plus_ShouldNotModifyOriginal()
    {
        int[] a = [1, 2];
        int[] b = [3];
        _ = a + b;

        Assert.Equal([1, 2], a);
        Assert.Equal([3], b);
    }

    [Fact]
    public void Plus_LeftEmpty()
    {
        int[] a = [];
        int[] b = [1, 2];
        var r = a + b;

        Assert.Equal([1, 2], r);
    }

    [Fact]
    public void Plus_RightEmpty()
    {
        int[] a = [1, 2];
        int[] b = [];
        var r = a + b;

        Assert.Equal([1, 2], r);
    }

    [Fact]
    public void Plus_BothEmpty()
    {
        int[] a = [];
        int[] b = [];
        var r = a + b;

        Assert.Empty(r);
    }

    [Fact]
    public void Plus_ReferenceType()
    {
        string[] a = ["a", "b"];
        string[] b = ["c"];
        var r = a + b;

        Assert.Equal(["a", "b", "c"], r);
    }

    [Fact]
    public void Plus_ShouldCreateNewArray()
    {
        int[] a = [1, 2];
        int[] b = [3, 4];
        var r = a + b;

        Assert.NotSame(a, r);
        Assert.NotSame(b, r);
    }

    [Fact]
    public void Plus_LengthCorrect()
    {
        int[] a = [1, 2, 3];
        int[] b = [4, 5];
        var r = a + b;

        Assert.Equal(5, r.Length);
    }

    [Fact]
    public void Plus_Chained()
    {
        int[] a = [1];
        int[] b = [2];
        int[] c = [3];
        var r = a + b + c;

        Assert.Equal([1, 2, 3], r);
    }

    [Fact]
    public void Plus_LargerArray()
    {
        var a = new int[100];
        var b = new int[200];

        for (var i = 0; i < 100; i++)
            a[i] = i;

        for (var i = 0; i < 200; i++)
            b[i] = i + 100;

        var r = a + b;

        Assert.Equal(300, r.Length);
        Assert.Equal(0, r[0]);
        Assert.Equal(99, r[99]);
        Assert.Equal(100, r[100]);
        Assert.Equal(299, r[299]);
    }

    [Fact]
    public void Plus_CopyOffsetCorrect()
    {
        int[] a = [10, 20];
        int[] b = [30, 40, 50];
        var r = a + b;

        Assert.Equal(10, r[0]);
        Assert.Equal(20, r[1]);
        Assert.Equal(30, r[2]);
        Assert.Equal(40, r[3]);
        Assert.Equal(50, r[4]);
    }
}
