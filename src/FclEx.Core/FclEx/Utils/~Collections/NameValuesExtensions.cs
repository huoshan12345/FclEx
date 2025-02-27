namespace FclEx.Utils;

public static class NameValuesExtensions
{
    public static TSelf Add<TSelf, T>(this TSelf self, string? key, T? value) where TSelf : NameValues<TSelf>
    {
        return self.Add(key, value?.ToString());
    }

    public static TSelf Add<TSelf>(this TSelf self, KeyValuePair<string?, string?> pair) where TSelf : NameValues<TSelf>
    {
        return self.Add(pair.Key, pair.Value);
    }

    public static TSelf Add<TSelf>(this TSelf self, Tuple<string?, string?> pair) where TSelf : NameValues<TSelf>
    {
        return self.Add(pair.Item1, pair.Item2);
    }

    public static TSelf Add<TSelf>(this TSelf self, (string?, string?) pair) where TSelf : NameValues<TSelf>
    {
        return self.Add(pair.Item1, pair.Item2);
    }

    public static TSelf Add<TSelf>(this TSelf self, IEnumerable<KeyValuePair<string, string>> enumerable) where TSelf : NameValues<TSelf>
    {
        foreach (var (key, value) in enumerable.EmptyIfNull())
        {
            self.Add(key, value);
        }
        return self;
    }

    public static TSelf Set<TSelf>(this TSelf self, IEnumerable<KeyValuePair<string, string>> enumerable) where TSelf : NameValues<TSelf>
    {
        foreach (var (key, value) in enumerable.EmptyIfNull())
        {
            self.Set(key, value);
        }
        return self;
    }

    public static TSelf Add<TSelf, T>(this TSelf self, T builder)
        where TSelf : NameValues<TSelf>
        where T : INameValuesBuilder
    {
        return self.Add(builder.Build());
    }

    public static TSelf Add<TSelf, T>(this TSelf self, IEnumerable<KeyValuePair<string, T>> pairs)
        where TSelf : NameValues<TSelf>
        where T : IEnumerable<string>
    {
        foreach (var (key, values) in pairs)
        {
            foreach (var value in values)
            {
                self.Add(key, value);
            }
        }
        return self;
    }

    public static TSelf Set<TSelf, T>(this TSelf self, IEnumerable<KeyValuePair<string, T>> pairs)
        where TSelf : NameValues<TSelf>
        where T : IEnumerable<string>
    {
        foreach (var (key, values) in pairs)
        {
            foreach (var value in values)
            {
                self.Set(key, value);
            }
        }
        return self;
    }
}