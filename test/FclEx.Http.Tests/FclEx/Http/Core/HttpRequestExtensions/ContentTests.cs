namespace FclEx.Http.Core.HttpRequestExtensions;

public class ContentTests
{
    [Fact]
    public async Task StringContent_CreatesUtf8StringContent()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var result = request.StringContent("hello");

        Assert.Same(request, result);
        Assert.IsType<StringContent>(request.Content);
        Assert.Equal("hello", await request.Content!.ReadAsStringAsync());
        Assert.Equal(Encoding.UTF8.WebName, request.Content.Headers.ContentType?.CharSet);
    }

    [Fact]
    public async Task Content_AssignsContentAndReturnsRequest()
    {
        var request = HttpRequest.Post("https://example.com/api");
        using var content = new StringContent("payload");

        var result = request.Content(content);

        Assert.Same(request, result);
        Assert.Same(content, request.Content);
        Assert.Equal("payload", await request.Content!.ReadAsStringAsync());
    }

    [Fact]
    public async Task StringContent_WhenEncodingIsProvided_UsesProvidedEncoding()
    {
        var request = HttpRequest.Post("https://example.com/api");

        request.StringContent("hello", Encoding.Unicode);

        Assert.Equal(Encoding.Unicode.WebName, request.Content!.Headers.ContentType?.CharSet);
        Assert.Equal("hello", await request.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ByteArrayContent_WithWholeArray_UsesAllBytes()
    {
        var request = HttpRequest.Post("https://example.com/api");
        byte[] bytes = [1, 2, 3];

        var result = request.ByteArrayContent(bytes);

        Assert.Same(request, result);
        Assert.Equal([1, 2, 3], await request.Content!.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ByteArrayContent_WithOffsetAndCount_UsesSpecifiedRange()
    {
        var request = HttpRequest.Post("https://example.com/api");
        byte[] bytes = [1, 2, 3, 4, 5];

        var result = request.ByteArrayContent(bytes, 2, 2);

        Assert.Same(request, result);
        Assert.Equal([3, 4], await request.Content!.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ByteArrayContent_WithArraySegment_UsesSegmentRange()
    {
        var request = HttpRequest.Post("https://example.com/api");
        byte[] bytes = [1, 2, 3, 4, 5];

        var result = request.ByteArrayContent(new ArraySegment<byte>(bytes, 1, 3));

        Assert.Same(request, result);
        Assert.Equal([2, 3, 4], await request.Content!.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ByteArrayContent_WithDefaultArraySegment_UsesEmptyArray()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var result = request.ByteArrayContent(default(ArraySegment<byte>));

        Assert.Same(request, result);
        Assert.Empty(await request.Content!.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task JsonContent_SerializesObjectAsJsonContent()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var result = request.JsonContent(new JsonModel("alice", 3));

        Assert.Same(request, result);
        Assert.Equal("""{"Name":"alice","Count":3}""", await request.Content!.ReadAsStringAsync());
        Assert.Equal(MediaTypes.Json, request.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task JsonContent_WithSerializerOptions_UsesOptions()
    {
        var request = HttpRequest.Post("https://example.com/api");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        request.JsonContent(new JsonModel("alice", 3), options);

        Assert.Equal("""{"name":"alice","count":3}""", await request.Content!.ReadAsStringAsync());
    }

    [Fact]
    public async Task JsonContent_WithJsonOptions_UsesResolvedOptions()
    {
        var request = HttpRequest.Post("https://example.com/api");

        request.JsonContent(new JsonModel("alice", 3), JsonOptions.Web);

        Assert.Equal("""{"name":"alice","count":3}""", await request.Content!.ReadAsStringAsync());
    }

    [Fact]
    public async Task FormContent_CreatesFormUrlEncodedContent()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var result = request.FormContent(
        [
            KeyValuePair.Create("name", "alice"),
            KeyValuePair.Create("city", "st john's"),
        ]);

        Assert.Same(request, result);
        Assert.IsType<FormUrlEncodedContent>(request.Content);
        Assert.Equal("name=alice&city=st+john%27s", await request.Content!.ReadAsStringAsync());
    }

    [Fact]
    public async Task CreateBufferedContentAsync_WhenContentExists_ReturnsBufferedContent()
    {
        var request = HttpRequest.Post("https://example.com/api")
            .StringContent("payload")
            .BufferSize(2)
            .ReadBufferTimeout(TimeSpan.FromSeconds(1));

        using var buffered = await request.CreateBufferedContentAsync();

        Assert.NotNull(buffered);
        Assert.Equal("payload", await buffered.ReadAsStringAsync());
    }

    [Fact]
    public async Task CreateBufferedContentAsync_WhenContentIsNull_ReturnsNull()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var buffered = await request.CreateBufferedContentAsync();

        Assert.Null(buffered);
    }

    private sealed record JsonModel(string Name, int Count);
}
