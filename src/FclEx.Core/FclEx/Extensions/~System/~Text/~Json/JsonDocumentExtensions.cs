namespace FclEx.Extensions;

public static class JsonDocumentExtensions
{
    public static T? ToObject<T>(this JsonDocument document, JsonSerializerOptions? options = null)
    {
        return document.Deserialize<T>(options ?? JsonHelper.GetOptions());
    }

    public static T? ToObject<T>(this JsonDocument document, JsonOptions options)
    {
        return document.ToObject<T>(JsonHelper.GetOptions(options));
    }

    extension(JsonDocument)
    {
        public static JsonDocument From<T>(T obj, JsonSerializerOptions? options = null)
        {
            options ??= JsonHelper.GetOptions();
            return JsonSerializer.SerializeToDocument(obj, options);
        }
    }
}
