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
    public void Constructor_Rejects_Null_Success_Value()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new OperationResult<object?>(null!, TimeSpan.Zero);
        });
    }

    [Fact]
    public void Success_Factory_Rejects_Null_Value()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = Operation.Success<object?>(null!);
        });
    }

    [Fact]
    public void Implicit_Success_Conversion_Rejects_Null_Value()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            OperationResult<object?> result = (object?)null!;
            _ = result;
        });
    }

    [Fact]
    public void Default_Value_Is_Not_A_Valid_Result()
    {
        OperationResult<object> result = default;

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Cast_FailedRuntimeCast_UsesActualSourceTypeInMessage()
    {
        var result = Operation.Success<object>(1).Cast<string>();

        Assert.False(result.IsSuccess);
        Assert.Contains(typeof(int).ToString(), result.Exception?.Message);
    }
}
