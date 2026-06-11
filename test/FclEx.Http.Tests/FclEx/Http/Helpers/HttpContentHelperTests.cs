namespace FclEx.Http.Helpers;

public class HttpContentHelperTests
{
    [Fact]
    public async Task FromJson_CreatesUtf8JsonStringContent()
    {
        using var content = HttpContentHelper.FromJson("""{"name":"fclex"}""");

        Assert.Equal("""{"name":"fclex"}""", await content.ReadAsStringAsync());
        Assert.Equal(MediaTypes.Json, content.Headers.ContentType?.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, content.Headers.ContentType?.CharSet);
    }

    [Fact]
    public async Task ToJsonContent_SerializesObjectWithProvidedOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        using var content = HttpContentHelper.ToJsonContent(new JsonModel("alice", 3), options);

        Assert.Equal("""{"name":"alice","count":3}""", await content.ReadAsStringAsync());
        Assert.Equal(MediaTypes.Json, content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ToGZipContent_CreatesGZipContentWithSourceContentType()
    {
        using var content = HttpContentHelper.ToGZipContent("payload", MediaTypes.Json);
        using var destination = new MemoryStream();

        await content.CopyToAsync(destination);

        Assert.Contains("gzip", content.Headers.ContentEncoding);
        Assert.Equal(MediaTypes.Json, content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(destination.ToArray());
    }

    private sealed record JsonModel(string Name, int Count);
}
