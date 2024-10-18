namespace FclEx.Extensions;

public static class JsonExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToJson(this object? obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsonNode? ToJsonNode(this string? str, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.SerializeToNode(str, options);
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

}