namespace FclEx.Extensions;

public static class JsonSerializerOptionsExtensions
{
    private static readonly Type _resolver = typeof(DefaultJsonTypeInfoResolver);
    private static readonly MethodInfo _getBuiltInConverter = _resolver.GetRequiredMethod("GetBuiltInConverter");
    private static readonly MethodInfo _createTypeInfoCore = _resolver.GetRequiredMethod("CreateTypeInfoCore");

    // this method will create actual converter for JsonConverterFactory.
    private static readonly MethodInfo _expandConverterFactory = typeof(JsonSerializerOptions).GetRequiredMethod("ExpandConverterFactory");

    /// <summary>
    /// Get <see cref="JsonTypeInfo{T}" /> with built-in <see cref="JsonConverter{T}" /> for the type <typeparamref name="T"/> to convert.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options"></param>
    /// <returns></returns>
    public static JsonTypeInfo<T> GetBuiltInJsonTypeInfo<T>(this JsonSerializerOptions options)
    {
        return (JsonTypeInfo<T>)options.GetBuiltInJsonTypeInfo(typeof(T));
    }

    /// <summary>
    /// Get <see cref="JsonTypeInfo" /> with built-in <see cref="JsonConverter" /> for the type to convert.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public static JsonTypeInfo GetBuiltInJsonTypeInfo(this JsonSerializerOptions options, Type typeToConvert)
    {
        var converter = _getBuiltInConverter.Invoke(null, [typeToConvert]);
        converter = _expandConverterFactory.Invoke(options, [converter, typeToConvert]);
        var typeInfo = _createTypeInfoCore.Invoke(null, [typeToConvert, converter, options]);
        return (JsonTypeInfo)typeInfo!;
    }

    /// <summary>
    /// Marks the current instance as read-only preventing any further user modification.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="populateMissingResolver">Populates unconfigured <see cref="TypeInfoResolver"/> properties with the reflection-based default.</param>
    public static JsonSerializerOptions ReadOnly(this JsonSerializerOptions options, bool populateMissingResolver)
    {
        options.MakeReadOnly(populateMissingResolver);
        return options;
    }

    public static JsonSerializerOptions AddConverters(this JsonSerializerOptions options, IEnumerable<JsonConverter> converters)
    {
        var readOnly = options.IsReadOnly;
        if (readOnly)
        {
            options = new(options);
        }

        options.Converters.AddRangeSafely(converters);
        if (readOnly)
        {
            options.MakeReadOnly(false);
        }
        return options;
    }

    private static JsonSerializerOptions AddModifier(this JsonSerializerOptions options, Action<JsonTypeInfo> modifier)
    {
        var actualOptions = options.IsReadOnly ? new(options) : options;
        var resolver = options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
        actualOptions.TypeInfoResolver = resolver.WithAddedModifier(modifier);

        if (options.IsReadOnly)
            actualOptions.MakeReadOnly(false);

        return actualOptions;
    }

    public static JsonSerializerOptions AddModifierForEmptyValue(this JsonSerializerOptions options)
    {
        return options.AddModifier(JsonHelper.IgnoreEmptyValue);
    }

    public static JsonSerializerOptions AddModifierForStaticMembers(this JsonSerializerOptions options)
    {
        return options.AddModifier(JsonHelper.IncludeStaticMembers);
    }

    private static readonly JsonSerializerOptions _default = JsonHelper.GetOptions();
    private static readonly JsonSerializerOptions _web = JsonHelper.GetOptions(JsonOptions.Web);

    extension(JsonSerializerOptions)
    {
        public static JsonSerializerOptions DefaultEx => _default;
        public static JsonSerializerOptions WebEx => _web;
    }
}