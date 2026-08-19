namespace FclEx.Extensions;

public static class JsonPropertyExtensions
{
    public static void Deconstruct(this JsonProperty property, out string name, out JsonElement value)
    {
        name = property.Name;
        value = property.Value;
    }
}
