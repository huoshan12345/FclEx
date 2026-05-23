namespace FclEx.Extensions;

public static partial class StringExtensions
{
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

    [MethodImpl(AggressiveInlining)]
    public static T? FromJson<T>(this string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(AggressiveInlining)]
    public static T? FromJson<T>(this string json, JsonOptions options)
    {
        return json.FromJson<T>(JsonHelper.GetOptions(options));
    }

    [MethodImpl(AggressiveInlining)]
    public static object? FromJson(this string json, Type type, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize(json, type, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(AggressiveInlining)]
    public static object? FromJson(this string json, Type type, JsonOptions options)
    {
        return json.FromJson(type, JsonHelper.GetOptions(options));
    }

    [MethodImpl(AggressiveInlining)]
    public static JsonNode? ToJsonNode(this string str, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<JsonNode>(str, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(AggressiveInlining)]
    public static JsonNode? ToJsonNode(this string str, JsonOptions options)
    {
        return str.ToJsonNode(JsonHelper.GetOptions(options));
    }

    [MethodImpl(AggressiveInlining)]
    public static JsonElement ToJsonElement(this string str, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<JsonElement>(str, options ?? JsonHelper.GetOptions());
    }

    [MethodImpl(AggressiveInlining)]
    public static JsonElement ToJsonElement(this string str, JsonOptions options)
    {
        return str.ToJsonElement(JsonHelper.GetOptions(options));
    }
}