namespace System.Text.Json;

public class FileSystemInfoJsonConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FileSystemInfoJsonConverter());
        return options;
    }

    [Fact]
    public void CanConvert_Should_Return_True_For_File_And_Directory()
    {
        var factory = new FileSystemInfoJsonConverter();

        Assert.True(factory.CanConvert(typeof(FileInfo)));
        Assert.True(factory.CanConvert(typeof(DirectoryInfo)));
    }

    [Fact]
    public void CanConvert_Should_Return_False_For_Other_Types()
    {
        var factory = new FileSystemInfoJsonConverter();

        Assert.False(factory.CanConvert(typeof(string)));
        Assert.False(factory.CanConvert(typeof(object)));
    }

    [Fact]
    public void Serialize_FileInfo_Should_Write_FullPath()
    {
        var options = CreateOptions();
        var file = new FileInfo("test.txt");

        var json = JsonSerializer.Serialize(file, options);

        Assert.Contains("test.txt", json);
        Assert.StartsWith("\"", json);
        Assert.EndsWith("\"", json);
    }

    [Fact]
    public void Serialize_DirectoryInfo_Should_Write_FullPath()
    {
        var options = CreateOptions();
        var dir = new DirectoryInfo("test_dir");

        var json = JsonSerializer.Serialize(dir, options);

        Assert.Contains("test_dir", json);
        Assert.StartsWith("\"", json);
        Assert.EndsWith("\"", json);
    }

    [Fact]
    public void Deserialize_FileInfo_Should_Create_Instance()
    {
        var options = CreateOptions();
        var json = "\"abc.txt\"";

        var file = JsonSerializer.Deserialize<FileInfo>(json, options);

        Assert.NotNull(file);
        Assert.Equal("abc.txt", file!.Name);
    }

    [Fact]
    public void Deserialize_DirectoryInfo_Should_Create_Instance()
    {
        var options = CreateOptions();
        var json = "\"mydir\"";

        var dir = JsonSerializer.Deserialize<DirectoryInfo>(json, options);

        Assert.NotNull(dir);
        Assert.Equal("mydir", dir!.Name);
    }

    [Fact]
    public void Deserialize_Should_Handle_Null()
    {
        var options = CreateOptions();

        var file = JsonSerializer.Deserialize<FileInfo>("null", options);
        var dir = JsonSerializer.Deserialize<DirectoryInfo>("null", options);

        Assert.Null(file);
        Assert.Null(dir);
    }

    [Fact]
    public void Deserialize_Should_Throw_On_Invalid_Token()
    {
        var options = CreateOptions();

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<FileInfo>("123", options));

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DirectoryInfo>("true", options));
    }

    [Fact]
    public void RoundTrip_FileInfo_Should_Preserve_Path()
    {
        var options = CreateOptions();
        var original = new FileInfo("foo/bar.txt");

        var json = JsonSerializer.Serialize(original, options);
        var result = JsonSerializer.Deserialize<FileInfo>(json, options);

        Assert.NotNull(result);
        Assert.Equal(original.FullName, result!.FullName);
    }

    [Fact]
    public void RoundTrip_DirectoryInfo_Should_Preserve_Path()
    {
        var options = CreateOptions();
        var original = new DirectoryInfo("foo/bar");

        var json = JsonSerializer.Serialize(original, options);
        var result = JsonSerializer.Deserialize<DirectoryInfo>(json, options);

        Assert.NotNull(result);
        Assert.Equal(original.FullName, result!.FullName);
    }
}
