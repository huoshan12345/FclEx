namespace FclEx.Utils;

public class SimpleExceptionTests
{
    private static void CheckSimpleException(Exception ex, bool noStackTrace)
    {
        Assert.IsType<SimpleException>(ex);

        if (noStackTrace)
        {
            Assert.Null(ex.StackTrace);
        }
        else
        {
            Assert.NotNullNorEmpty(ex.StackTrace);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void New_Test(bool noStackTrace)
    {
        var ex = new SimpleException("test", noStackTrace);
        CheckSimpleException(ex, noStackTrace);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void New_Inner_Test(bool noStackTrace)
    {
        try
        {
            throw new Exception("inner");
        }
        catch (Exception ex)
        {
            var e = new SimpleException("test", ex, noStackTrace);
            CheckSimpleException(e, noStackTrace);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Throw_Test(bool noStackTrace)
    {
        try
        {
            throw new SimpleException("test", noStackTrace);
        }
        catch (Exception ex)
        {
            CheckSimpleException(ex, noStackTrace);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Throw_Inner_Test(bool noStackTrace)
    {
        try
        {
            throw new Exception("inner");
        }
        catch (Exception ex)
        {
            try
            {
                throw new SimpleException("test", ex, noStackTrace);
            }
            catch (Exception e)
            {
                CheckSimpleException(e, noStackTrace);
            }
        }
    }
}