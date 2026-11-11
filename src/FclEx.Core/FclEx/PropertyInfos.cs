namespace FclEx;

public static class PropertyInfos
{
    public static class JsonTypeInfoResolver
    {
        private static readonly Lazy<PropertyInfo> _empty = new(() => typeof(System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver).GetRequiredProperty("Empty"));

        public static PropertyInfo Empty => _empty.Value;
    }
}