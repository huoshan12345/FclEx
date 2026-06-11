namespace FclEx.Http.Extensions;

public class StringBuilderExtensionsTests
{
    [Fact]
    public void AppendHttpLine_AppendsValueAndCrLf()
    {
        var builder = new StringBuilder();

        var result = builder.AppendHttpLine("GET / HTTP/1.1");

        Assert.Same(builder, result);
        Assert.Equal("GET / HTTP/1.1\r\n", builder.ToString());
    }
}
