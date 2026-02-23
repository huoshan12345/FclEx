namespace FclEx.Utils.OperationResult;

public partial class OperationResultTests
{
    [Fact]
    public void ImplicitOperator_FromData()
    {
        const int expected = 1;
        var r = Test(expected);
        Assert.True(r.IsSuccess);
        Assert.Equal(expected, r.Value);

        OperationResult<int> Test(int input)
        {
            return input;
        }
    }

    [Fact]
    public void ImplicitOperator_FromString()
    {
        var r = Test();
        Assert.False(r.IsSuccess);
        Assert.IsType<SimpleException>(r.Exception);

        OperationResult<int> Test()
        {
            return "";
        }
    }

    [Fact]
    public void ImplicitOperator_FromException()
    {
        var ex = new ArgumentException();
        var r = Test(ex);
        Assert.False(r.IsSuccess);
        Assert.IsType(ex.GetType(), r.Exception);

        OperationResult<int> Test(Exception e)
        {
            return ex;
        }
    }
}