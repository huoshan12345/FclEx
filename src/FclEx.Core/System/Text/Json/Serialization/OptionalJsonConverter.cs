namespace System.Text.Json.Serialization;

/// <summary>
/// Converts <see cref="Optional{T}"/> values to and from the JSON representation of their contained values.
/// </summary>
/// <remarks>
/// An optional with a value is serialized as that value without an additional wrapper object. An optional with no
/// value is serialized as JSON <see langword="null"/>, and JSON <see langword="null"/> is deserialized as an optional
/// with no value. Conversion of a contained value is delegated to the converter configured for its type.
/// </remarks>
public sealed class OptionalJsonConverter : JsonConverterFactory
{
    /// <summary>
    /// Gets a reusable converter instance.
    /// </summary>
    public static readonly OptionalJsonConverter Instance = new();

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
               && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (CanConvert(typeToConvert) == false)
            throw new InvalidOperationException($"{typeToConvert} is not an {typeof(Optional<>)} type.");

        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Optional<T>(value);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (value.HasValue == false)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
