namespace System.Text.Json.Serialization;

/// <summary>
/// A custom JSON converter that deserializes any JSON value into its string representation.
/// </summary>
/// <remarks>
/// This converter is useful when the incoming JSON may contain mixed data types (e.g., string, number, boolean, object),
/// but the target property should always be a string.  
/// 
/// Behavior:
/// <see cref="JsonTokenType.String"/> → returns the string as is.<br/>
/// <see cref="JsonTokenType.Number"/> → returns the number as a string (integer or double format).<br/>
/// <see cref="JsonTokenType.True"/> / <see cref="JsonTokenType.False"/> → returns "<see langword="true"/>" or "<see langword="false"/>".<br/>
/// <see cref="JsonTokenType.Null"/> → returns <c><see langword="null"/></c>.<br/>
/// Other JSON types (arrays, objects, etc.) → returns their raw JSON text.<br/>
/// </remarks>
public class ReadAsStringJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            _ => ReadOther(ref reader),
        };

        static string ReadNumber(ref Utf8JsonReader reader)
        {
            return reader.TryGetInt64(out var longValue)
                ? longValue.ToString()
                : reader.GetDouble().ToString(CultureInfo.InvariantCulture);
        }

        static string ReadOther(ref Utf8JsonReader reader)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.GetRawText();
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }
}
