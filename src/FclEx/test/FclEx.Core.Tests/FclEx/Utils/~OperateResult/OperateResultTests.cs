namespace FclEx.Utils;

public partial class OperateResultTests
{
    [Fact]
    public void ImplicitOperator_FromData()
    {
        const int expected = 1;
        var r = Test(expected);
        Assert.True(r.Success);
        Assert.Equal(expected, r.Value);

        OperateResult<int> Test(int input)
        {
            return input;
        }
    }

    [Fact]
    public void ImplicitOperator_FromString()
    {
        var r = Test();
        Assert.False(r.Success);
        Assert.IsType<SimpleException>(r.Exception);

        OperateResult<int> Test()
        {
            return "";
        }
    }

    [Fact]
    public void ImplicitOperator_FromException()
    {
        var ex = new ArgumentException();
        var r = Test(ex);
        Assert.False(r.Success);
        Assert.IsType(ex.GetType(), r.Exception);

        OperateResult<int> Test(Exception e)
        {
            return ex;
        }
    }
}