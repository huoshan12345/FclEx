#if NET5_0_OR_GREATER
namespace FclEx.Http.Extensions;

public class HttpRequestOptionsExtensionsTests
{
    [Fact]
    public void Set_StoresValueUnderTypedOptionsKey()
    {
        var options = new HttpRequestOptions();

        options.Set("retry-count", 3);

        Assert.True(options.TryGetValue(new HttpRequestOptionsKey<int>("retry-count"), out var value));
        Assert.Equal(3, value);
    }
}
#endif
