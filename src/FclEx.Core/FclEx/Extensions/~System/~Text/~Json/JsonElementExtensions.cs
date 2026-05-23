namespace FclEx.Extensions;

public static class JsonElementExtensions
{
    public static bool HasProperty(this JsonElement element, string name)
    {
        return element.TryGetProperty(name, out _);
    }

    extension(JsonElement)
    {
        public static JsonElement From<T>(T obj, JsonSerializerOptions? options = null)
        {
            options ??= JsonHelper.GetOptions();
            return JsonSerializer.SerializeToElement(obj, options);
        }
    }
}