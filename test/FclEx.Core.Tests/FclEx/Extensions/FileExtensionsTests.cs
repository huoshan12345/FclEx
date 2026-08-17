namespace FclEx.Extensions;

public class FileExtensionsTests
{
    [Fact]
    public async Task WriteAllTextAsync_WithDefaultEncoding_WritesUtf8WithoutBom()
    {
        var path = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(path, "text");

            var bytes = File.ReadAllBytes(path);
            Assert.False(bytes.Take(Encoding.UTF8.GetPreamble().Length).SequenceEqual(Encoding.UTF8.GetPreamble()));
            Assert.Equal(Encoding.UTF8.GetBytes("text"), bytes);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
