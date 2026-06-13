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

    [Fact]
    public void Set_WhenSameKeyIsUsedWithDifferentValueType_ReplacesExistingValue()
    {
        var options = new HttpRequestOptions();

        options.Set("value", 3);
        options.Set("value", "three");

        Assert.True(options.TryGetValue(new HttpRequestOptionsKey<string>("value"), out var text));
        Assert.False(options.TryGetValue(new HttpRequestOptionsKey<int>("value"), out _));
        Assert.Equal("three", text);
    }
}
#endif
