namespace FclEx;

public static class PropertyInfos
{
    public static class JsonTypeInfoResolver
    {
        private static readonly Lazy<PropertyInfo> _empty = new(() => typeof(JsonTypeInfoResolver).GetRequiredProperty("Empty"));

        public static PropertyInfo Empty => _empty.Value;
    }

    public static readonly PropertyInfo JsonTypeInfoResolver_Empty =
        typeof(JsonTypeInfoResolver).GetRequiredProperty("Empty");
}