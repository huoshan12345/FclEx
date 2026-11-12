#if NET8_0_OR_GREATER
using System.Net.Http;
using System.Net.NetworkInformation;

namespace FclEx;

public class UnsafeAccessorsTests
{
    [Fact]
    public void ExceptionReferences_UpdateMessageAndStackTrace()
    {
        var exception = new InvalidOperationException("before");
        UnsafeAccessors.Exception_Message(exception) = "after";
        UnsafeAccessors.Exception_StackTrace(exception) = "custom stack";
        Assert.Equal("after", exception.Message);
        Assert.Equal("custom stack", exception.StackTrace);
        UnsafeAccessors.Exception_Message(exception) = null;
        UnsafeAccessors.Exception_StackTrace(exception) = null;
        Assert.Null(exception.GetMessage());
        Assert.Null(exception.GetStackTrace());
    }

    [Fact]
    public void AddressReferences_ExposeBackingArrays()
    {
        var physical = new PhysicalAddress([0x01, 0x02]);
        var bytes = UnsafeAccessors.PhysicalAddress_Address(physical);
        bytes[1] = 0x03;
        Assert.Equal(new byte[] { 0x01, 0x03 }, physical.GetAddressBytes());

        var ip = IPAddress.Parse("2001:db8::1");
        var numbers = UnsafeAccessors.IPAddress_Numbers(ip);
        Assert.Equal(new ushort[] { 0x2001, 0x0db8, 0, 0, 0, 0, 0, 1 }, numbers);
        Assert.Same(numbers, UnsafeAccessors.IPAddress_Numbers(ip));
        Assert.Null(UnsafeAccessors.IPAddress_Numbers(IPAddress.Loopback));
    }

    [Fact]
    public void HandlerReference_UpdatesRootHandler()
    {
        using var original = new HttpClientHandler();
        using var replacement = new HttpClientHandler();
        using var invoker = new HttpMessageInvoker(original, disposeHandler: false);
        Assert.Same(original, UnsafeAccessors.HttpMessageInvoker_Handler(invoker));
        UnsafeAccessors.HttpMessageInvoker_Handler(invoker) = replacement;
        Assert.Same(replacement, FieldInfos.HttpMessageInvoker_Handler.GetValue(invoker));
    }

#if !NET11_0_OR_GREATER
    [Fact]
    public async Task SendStatusReference_AllowsCompletedRequestToBeSentAgain()
    {
        using var client = new HttpClient(new SuccessfulHandler());
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        using var first = await client.SendAsync(request, CancellationToken.None);
        Assert.NotEqual(0, UnsafeAccessors.HttpRequestMessage_SendStatus(request));
        UnsafeAccessors.HttpRequestMessage_SendStatus(request) = 0;
        using var second = await client.SendAsync(request, CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    private sealed class SuccessfulHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
#endif
}
#endif
