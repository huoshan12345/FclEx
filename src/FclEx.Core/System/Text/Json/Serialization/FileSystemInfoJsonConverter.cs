namespace System.Text.Json.Serialization;

public sealed class FileSystemInfoJsonConverter : JsonConverterFactory
{
    public static readonly FileSystemInfoJsonConverter Instance = new();

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(FileSystemInfo).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(FileInfo))
            return FileInfoJsonConverter.Instance;

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (typeToConvert == typeof(DirectoryInfo))
            return DirectoryInfoJsonConverter.Instance;

        throw new NotSupportedException(typeToConvert.Name);
    }
}
