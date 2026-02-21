namespace FclEx.Extensions;

public static class JsonDocumentExtensions
{
    extension(JsonDocument)
    {
        public static JsonDocument From<T>(T obj, JsonSerializerOptions? options = null)
        {
            options ??= JsonHelper.GetOptions();
            return JsonSerializer.SerializeToDocument(obj, options);
        }
    }
}
