namespace FclEx.Extensions;

public static class NameValueCollectionExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> Enumerate(this NameValueCollection col)
    {
        var q = from k in col.AllKeys.NotNull()
                from v in col.GetValues(k).EmptyIfNull()
                select KvPair.Create(k, v);
        return q;
    }

    public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc, DupPolicy policy = DupPolicy.OnlyLast)
    {
        if (policy == DupPolicy.Array)
            throw new NotSupportedException();

        var dic = new Dictionary<string, string>(nvc.Count);
        foreach (var (k, v) in nvc.Enumerate())
        {
            switch (policy)
            {
                case DupPolicy.OnlyLast:
                {
                    dic[k] = v;
                    break;
                }
                case DupPolicy.OnlyFirst:
                {
                    dic.TryAdd(k, v);
                    break;
                }
                case DupPolicy.Throw:
                {
                    if (dic.TryGetValue(k, out var old))
                    {
                        if (old != v)
                            throw new ArgumentException($"duplicate key: {k} with different values: {old},{v}");
                    }
                    else
                    {
                        dic.Add(k, v);
                    }
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
            }
        }
        return dic;
    }

    public static JsonObject ToJObject(this NameValueCollection col, DupPolicy policy = DupPolicy.OnlyLast)
    {
        var obj = new JsonObject();
        foreach (var k in col.AllKeys.NotNull())
        {
            var values = col.GetValues(k).EmptyIfNull().ToHashSet();
            if (values.Count > 0)
                obj.Add(k, values.ToJToken(policy));
        }
        return obj;
    }

    internal static JsonNode? ToJToken(this ISet<string> values, DupPolicy policy)
    {
        Check.NotNull(values);
        Check.NotEmpty(values);

        if (values.Count == 1) return JsonSerializer.SerializeToNode(values.First());
        switch (policy)
        {
            case DupPolicy.OnlyLast: return JsonSerializer.SerializeToNode(values.Last());
            case DupPolicy.OnlyFirst: return JsonSerializer.SerializeToNode(values.First());
            case DupPolicy.Array: return JsonSerializer.SerializeToNode(values);
            case DupPolicy.Throw: throw new InvalidOperationException("the collection contains more than one value");
            default: throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
        }
    }

    public static bool IsValid([NotNullWhen(true)] this NameValueCollection? col)
    {
        return col?.Count > 0;
    }

}