namespace FclEx.Utils;

public class FormattedExceptionTests
{
    [Fact]
    public void Constructor_Should_Use_Wrapped_Exception_As_InnerException()
    {
        var innerException = new InvalidOperationException("failure");

        var exception = new FormattedException(innerException);

        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(innerException.Message, exception.Message);
        Assert.Equal(innerException.ToFormattedString(), exception.ToString());
    }

    [Fact]
    public void Constructor_Null_Exception_Should_Throw()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new FormattedException(null!));

        Assert.Equal("exception", exception.ParamName);
    }
}
