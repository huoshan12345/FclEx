namespace FclEx.Utils;

public partial class OperationResultTests
{
    [Fact]
    public void ImplicitOperator_FromData()
    {
        const int expected = 1;
        var r = Test(expected);
        Assert.True(r.IsSuccess);
        Assert.Equal(expected, r.Value);

        static OperationResult<int> Test(int input)
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

        static OperationResult<int> Test()
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

        static OperationResult<int> Test(Exception e)
        {
            return e;
        }
    }

    [Fact]
    public void Cast_NullSuccess_ToReferenceTarget_ReturnsSuccess()
    {
        var result = Operation.Success<object?>(null).Cast<string?>();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Cast_NullSuccess_ToNonNullableValueTarget_ReturnsError()
    {
        var result = Operation.Success<object?>(null).Cast<int>();

        Assert.False(result.IsSuccess);
        Assert.IsType<InvalidCastException>(result.Exception);
    }

    [Fact]
    public void Cast_FailedRuntimeCast_UsesActualSourceTypeInMessage()
    {
        var result = Operation.Success<object>(1).Cast<string>();

        Assert.False(result.IsSuccess);
        Assert.Contains(typeof(int).ToString(), result.Exception?.Message);
    }
}
