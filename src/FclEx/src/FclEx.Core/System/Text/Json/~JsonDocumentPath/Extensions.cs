namespace System.Text.Json;

internal static class Extensions
{
    public static bool IsValue(this JsonElement src)
    {
        return src.ValueKind
            is JsonValueKind.False
            or JsonValueKind.True
            or JsonValueKind.String
            or JsonValueKind.Number
            or JsonValueKind.Null
            or JsonValueKind.Undefined;
    }
    public static bool IsContainer(this JsonElement src)
    {
        return src.ValueKind is JsonValueKind.Array or JsonValueKind.Object;
    }
    public static bool IsContainer(this JsonElement? src)
    {
        return src.HasValue && src.Value.IsContainer();
    }

    public static bool TryGetFirstFromObject(this JsonElement? src, out JsonProperty? element)
    {
        element = null;
        return src.HasValue && src.Value.TryGetFirstFromObject(out element);
    }

    public static bool TryMoveNextFromObject(this JsonElement src, int cycle, out JsonProperty? element)
    {
        element = null;
        if (src.ValueKind == JsonValueKind.Object)
        {
            var currentObject = src.EnumerateObject();
            for (var i = 0; i < cycle; i++)
            {
                currentObject.MoveNext();
            }
            element = currentObject.Current;
            return true;
        }
        return false;
    }

    public static bool TryGetFirstFromObject(this JsonElement src, out JsonProperty? element)
    {
        element = null;
        if (src.ValueKind == JsonValueKind.Object)
        {
            var currentObject = src.EnumerateObject();
            if (currentObject.MoveNext())
            {
                element = currentObject.Current;
                return true;
            }
        }
        return false;
    }

    public static bool TryGetFirstFromArray(this JsonElement? src, out JsonElement? element)
    {
        element = null;
        return src.HasValue && src.Value.TryGetFirstFromArray(out element);
    }

    public static bool TryGetFirstFromArray(this JsonElement src, out JsonElement? element)
    {
        element = null;
        if (src.ValueKind == JsonValueKind.Array && src.GetArrayLength() > 0)
        {
            if (src.EnumerateArray().MoveNext())
            {
                element = src.EnumerateArray().Current;
                return true;
            }
        }
        return false;
    }

    public static IEnumerable<JsonElement> DescendantsAndSelf(this IEnumerable<JsonElement> source)
    {
        return source.SelectMany(j => j.DescendantsAndSelf());
    }

    public static IEnumerable<JsonElement> DescendantElements(this JsonElement src)
    {
        return GetDescendantElementsCore(src, false);
    }

    public static IEnumerable<JsonElement> DescendantsAndSelf(this JsonElement src)
    {
        return GetDescendantElementsCore(src, true);
    }

    public static IEnumerable<JsonElement> ChildrenTokens(this JsonElement src)
    {
        if (src.ValueKind == JsonValueKind.Object)
        {
            foreach (var item in src.EnumerateObject())
            {
                yield return item.Value;
            }
        }

        if (src.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in src.EnumerateArray())
            {
                yield return item;
            }
        }
    }

    internal static IEnumerable<JsonElement> GetDescendantElementsCore(JsonElement src, bool self)
    {
        if (self)
        {
            yield return src;
        }

        foreach (var o in src.ChildrenTokens())
        {
            yield return o;
            if (o.IsContainer())
            {
                foreach (var d in o.DescendantElements())
                {
                    yield return d;
                }
            }
        }
    }

    public static IEnumerable<JsonProperty> GetDescendantProperties(this JsonElement src)
    {
        return GetDescendantPropertiesCore(src);
    }

    internal static IEnumerable<JsonProperty> GetDescendantPropertiesCore(JsonElement src)
    {
        foreach (var o in src.ChildrenPropertiesCore())
        {
            yield return o;
            if (o.Value.IsContainer())
            {
                foreach (var d in o.Value.GetDescendantProperties())
                {
                    yield return d;
                }
            }
        }
    }

    internal static IEnumerable<JsonProperty> ChildrenPropertiesCore(this JsonElement src)
    {
        if (src.ValueKind == JsonValueKind.Object)
        {
            foreach (var item in src.EnumerateObject())
            {
                yield return item;
            }
        }

        if (src.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in src.EnumerateArray())
            {
                foreach (var o in item.ChildrenPropertiesCore())
                {
                    yield return o;
                }
            }
        }
    }

    public static int CompareTo(this JsonElement value, JsonElement queryValue)
    {
        var comparisonType = (value.ValueKind == JsonValueKind.String && value.ValueKind != queryValue.ValueKind)
            ? queryValue.ValueKind
            : value.ValueKind;
        return Compare(comparisonType, value, queryValue);
    }

    private static int Compare(JsonValueKind valueType, JsonElement objA, JsonElement objB)
    {
        /*Same types*/
        if (objA.ValueKind == JsonValueKind.Null && objB.ValueKind == JsonValueKind.Null)
        {
            return 0;
        }
        if (objA.ValueKind == JsonValueKind.Undefined && objB.ValueKind == JsonValueKind.Undefined)
        {
            return 0;
        }
        if (objA.ValueKind == JsonValueKind.True && objB.ValueKind == JsonValueKind.True)
        {
            return 0;
        }
        if (objA.ValueKind == JsonValueKind.False && objB.ValueKind == JsonValueKind.False)
        {
            return 0;
        }
        if (objA.ValueKind == JsonValueKind.Number && objB.ValueKind == JsonValueKind.Number)
        {
            return objA.GetDouble().CompareTo(objB.GetDouble());
        }
        if (objA.ValueKind == JsonValueKind.String && objB.ValueKind == JsonValueKind.String)
        {
            return string.Compare(objA.GetString(), objB.GetString(), StringComparison.Ordinal);
        }
        //When objA is a number and objB is not.
        if (objA.ValueKind == JsonValueKind.Number)
        {
            var valueObjA = objA.GetDouble();
            if (objB.ValueKind == JsonValueKind.String)
            {
                if (double.TryParse(objB.GetRawText().Trim('"'), out var queryValueTyped))
                {
                    return valueObjA.CompareTo(queryValueTyped);
                }
            }
        }
        //When objA is a string and objB is not.
        if (objA.ValueKind == JsonValueKind.String)
        {
            if (objB.ValueKind == JsonValueKind.Number)
            {
                if (double.TryParse(objA.GetRawText().Trim('"'), out var valueTyped))
                {
                    return valueTyped.CompareTo(objB.GetDouble());
                }
            }
        }
        return -1;
    }
}