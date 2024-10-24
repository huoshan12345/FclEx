namespace FclEx.Extensions;

public static class JsonExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? FromJson<T>(this string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? FromJson<T>(this string json, JsonOptions options)
    {
        return JsonSerializer.Deserialize<T>(json, JsonHelper.GetOptions(options));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? FromJson(this string json, Type type, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize(json, type, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJson(this object? obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJson(this object? obj, JsonOptions options)
    {
        return obj.ToJson(JsonHelper.GetOptions(options));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJsonCamelCase(this object? obj)
    {
        return obj.ToJson(new JsonOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonNode? ToJsonNode(this string str, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<JsonNode>(str, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonNode? ToJsonNode<T>(this T? value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.SerializeToNode(value, options ?? JsonHelper.GetOptions());
    }

    public static bool IsPossibleJson([NotNullWhen(true)] this string? str)
    {
        /*
         * In JSON, values must be one of the following data types:
            a string
            a number
            an object (JSON object)
            an array
            a boolean
            null
         */

        if (str.IsNullOrEmpty())
            return false;

        switch (str.Length)
        {
            case 1 when str[0].IsDigit(): // a single digit
            case >= 2 when str == "null":            // null
            case >= 2 when str is "true" or "false": // a boolean
                return true;
            case >= 2:
            {
                var (first, last) = (str.First(), str.Last());
                switch (first)
                {
                    case '{' when last == '}': // an object
                    case '[' when last == ']': // an array
                    case '"' when last == '"': // a string
                        return true;
                }

                if (first.IsDigit() && last.IsDigit())
                    return true; // a positive number
                if (str.Length >= 3 && first == '-' && str[1].IsDigit() && last.IsDigit())
                    return true; // a negative number
                break;
            }
        }
        return false;
    }

    public static JsonNode? ReadNode(ref this Utf8JsonReader reader, JsonNodeOptions? options = null)
    {
        return JsonNode.Parse(ref reader, options);
    }

    public static JsonNode? ReadNode(ref this Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return JsonNode.Parse(ref reader, new() { PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive });
    }

    public static JsonElement ReadElement(ref this Utf8JsonReader reader)
    {
        return JsonElement.ParseValue(ref reader);
    }

    public static JsonNode? ToJsonNode(this JsonElement element, JsonNodeOptions? options = null)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Array => JsonArray.Create(element, options),
            JsonValueKind.Object => JsonObject.Create(element, options),
            _ => JsonValue.Create(element, options),
        };
    }

    public static string ToJsonString(this JsonNode node, JsonOptions options)
    {
        return node.ToJsonString(JsonHelper.GetOptions(options));
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

    public static void Deconstruct(this JsonProperty property, out string name, out JsonElement value)
    {
        name = property.Name;
        value = property.Value;
    }
}