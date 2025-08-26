namespace FclEx.Extensions;

public static class JsonElementExtensions
{
    public static bool HasProperty(this JsonElement element, string name)
    {
        return element.TryGetProperty(name, out _);
    }
}