namespace FclEx.Extensions;

public partial class ExceptionExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("replacement")]
    public void SetMessage_UpdatesRawStorageAndReturnsSameException(string? message)
    {
        var exception = new Exception("original");
        Assert.Same(exception, exception.SetMessage(message));
        Assert.Equal(message, exception.GetMessage());
        Assert.Equal(message ?? new Exception().Message, exception.Message);
    }

    [Fact]
    public void RawAccessors_BypassVirtualPropertyOverrides()
    {
        var exception = new OverridingException();
        exception.SetMessage("raw message").SetStackTrace("raw stack");
        Assert.Equal("raw message", exception.GetMessage());
        Assert.Equal("raw stack", exception.GetStackTrace());
        Assert.Equal("virtual message", exception.Message);
        Assert.Equal("virtual stack", exception.StackTrace);
    }

    [Theory]
    [InlineData("")]
    [InlineData("custom stack")]
    public void SetStackTrace_StoresExplicitStringAndReturnsSameException(string stackTrace)
    {
        var exception = new Exception();
        Assert.Null(exception.GetStackTrace());
        Assert.Same(exception, exception.SetStackTrace(stackTrace: stackTrace));
        Assert.Equal(stackTrace, exception.GetStackTrace());
        Assert.Equal(stackTrace, exception.StackTrace);
    }

    [Fact]
    public void SetStackTrace_WhenOmitted_CapturesCaller()
    {
        var exception = new Exception();
        exception.SetStackTrace();
        Assert.Contains(nameof(SetStackTrace_WhenOmitted_CapturesCaller), exception.GetStackTrace());
    }

    [Fact]
    public void SetMessage_CallbackUsesCurrentException()
    {
        var exception = new Exception("original");
        Assert.Same(exception, exception.SetMessage(ex => ex.Message + " updated"));
        Assert.Equal("original updated", exception.GetMessage());
    }

    private sealed class OverridingException : Exception
    {
        public override string Message => "virtual message";
        public override string StackTrace => "virtual stack";
    }
}
