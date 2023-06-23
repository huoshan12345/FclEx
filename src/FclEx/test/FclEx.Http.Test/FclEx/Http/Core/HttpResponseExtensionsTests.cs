namespace FclEx.Http.Core;

public class HttpResponseExtensionsTests
{
    [Fact]
    public async Task Task_HttpResponse_ThrowIfError_Test()
    {
        var error = nameof(Task_HttpResponse_ThrowIfError_Test);
        var task = Task.Run(async () =>
        {
            await Task.Yield();
            return HttpResponse.CreateError(HttpRequest.Get("http://localhost"), new SimpleException(error));
        });

        var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
        Assert.Equal(error, ex.Message);
    }
}