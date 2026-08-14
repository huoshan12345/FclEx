namespace System.Text.Json.Serialization;

/// <summary>
/// Allows sequence collection types to be read from either a JSON array or a single JSON value.
/// </summary>
/// <remarks>
/// Writing retains the behavior of the next configured converter for the collection type. Register this converter
/// in <see cref="JsonSerializerOptions.Converters" /> or on a property. Applying it to a collection type itself with
/// <see cref="JsonConverterAttribute" /> is not supported because public System.Text.Json APIs cannot bypass a
/// converter declared on the target type.
/// Strings, dictionaries, multidimensional arrays, and non-generic enumerable types are not treated as sequences.
/// </remarks>
public sealed class ReadAsArrayJsonConverter : JsonConverterFactory
{
    public static readonly ReadAsArrayJsonConverter Instance = new();

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(string) || IsDictionary(typeToConvert))
            return false;

        if (typeToConvert.IsArray)
            return typeToConvert.GetArrayRank() == 1;

        return typeToConvert.GetInterfaces().Prepend(typeToConvert).Any(static candidate =>
            candidate.IsGenericType
            && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (CanConvert(typeToConvert) == false)
            throw new InvalidOperationException($"{typeToConvert} is not a supported sequence collection type.");

        var converterAttribute = typeToConvert.GetCustomAttribute<JsonConverterAttribute>(inherit: false);
        if (converterAttribute?.ConverterType == typeof(ReadAsArrayJsonConverter))
        {
            throw new NotSupportedException(
                $"Apply {nameof(ReadAsArrayJsonConverter)} to a property or register it in "
                + $"{nameof(JsonSerializerOptions)}.{nameof(JsonSerializerOptions.Converters)}; applying it to "
                + $"the collection type {typeToConvert} cannot be composed with that type's default converter.");
        }

        var fallbackOptions = CreateFallbackOptions(options, typeToConvert);
        var converterType = typeof(SequenceConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, fallbackOptions)!;
    }

    private static bool IsDictionary(Type type)
    {
        return type.GetInterfaces().Prepend(type).Any(static candidate =>
            candidate.IsGenericType
            && (candidate.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                || candidate.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
    }

    private static JsonSerializerOptions CreateFallbackOptions(JsonSerializerOptions options, Type excludedType)
    {
        var fallbackOptions = new JsonSerializerOptions(options);

        for (var i = 0; i < fallbackOptions.Converters.Count; i++)
        {
            fallbackOptions.Converters[i] = fallbackOptions.Converters[i] switch
            {
                ReadAsArrayJsonConverter converter => new DelegatingConverterFactory(converter, [excludedType]),
                DelegatingConverterFactory converter => converter.Excluding(excludedType),
                var converter => converter,
            };
        }

        return fallbackOptions;
    }

    private sealed class DelegatingConverterFactory : JsonConverterFactory
    {
        private readonly ReadAsArrayJsonConverter _converter;
        private readonly HashSet<Type> _excludedTypes;

        public DelegatingConverterFactory(ReadAsArrayJsonConverter converter, IEnumerable<Type> excludedTypes)
        {
            _converter = converter;
            _excludedTypes = [.. excludedTypes];
        }

        public override bool CanConvert(Type typeToConvert)
            => _excludedTypes.Contains(typeToConvert) == false && _converter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => _converter.CreateConverter(typeToConvert, options);

        public DelegatingConverterFactory Excluding(Type type)
        {
            var excludedTypes = new HashSet<Type>(_excludedTypes) { type };
            return new DelegatingConverterFactory(_converter, excludedTypes);
        }
    }

    private sealed class SequenceConverter<TCollection> : JsonConverter<TCollection>
    {
        private readonly JsonSerializerOptions _fallbackOptions;

        public SequenceConverter(JsonSerializerOptions fallbackOptions)
        {
            _fallbackOptions = fallbackOptions;
        }

        public override bool HandleNull => true;

        public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.Null)
                return JsonSerializer.Deserialize<TCollection>(ref reader, _fallbackOptions);

            var token = reader.ReadElement();
            var arrayToken = new JsonArray(token.ToJsonNode());
            return arrayToken.Deserialize<TCollection>(_fallbackOptions);
        }

        public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, _fallbackOptions);
    }
}
