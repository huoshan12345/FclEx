namespace FclEx;

/// <summary>Provides lazily resolved descriptors for non-public runtime properties.</summary>
/// <remarks>Descriptors depend on implementation details and can fail to resolve after runtime changes or trimming.</remarks>
public static class PropertyInfos
{
    /// <summary>Provides descriptors for System.Text.Json's built-in resolver implementation.</summary>
    public static class JsonTypeInfoResolver
    {
        private static readonly Lazy<PropertyInfo> _empty = new(() => typeof(System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver).GetRequiredProperty("Empty"));

        /// <summary>The internal static property returning the shared resolver that supplies no JSON type metadata.</summary>
        /// <value>The same cached property descriptor on subsequent accesses; read it with a null target.</value>
        public static PropertyInfo Empty => _empty.Value;
    }
}