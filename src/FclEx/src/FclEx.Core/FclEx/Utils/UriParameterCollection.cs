namespace FclEx.Utils;

public sealed class UriParameterCollection : IReadOnlyCollection<KeyValuePair<string, string>>
{
    private readonly NameValueCollection _entries;

    public UriParameterCollection(string? query = null)
    {
        _entries = HttpUtility.ParseQueryString(query ?? "");
    }

    public override string ToString() => _entries.ToString() ?? "";

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _entries.Enumerate().GetEnumerator();
    }

    public UriParameterCollection Add(string? key, string? value)
    {
        if (key.IsNotEmpty())
        {
            _entries.Add(key, value ?? "");
        }
        return this;
    }

    public UriParameterCollection Set(string? key, string? value)
    {
        if (key.IsNotEmpty())
        {
            _entries.Set(key, value ?? "");
        }
        return this;
    }

    public UriParameterCollection Remove(string? name)
    {
        _entries.Remove(name);
        return this;
    }

    public string Get(string? name) => _entries.Get(name) ?? "";
    public string[] GetValues(string? name) => _entries.GetValues(name) ?? [];

    public int Count => _entries.Count;
}

public static class UriParameterCollectionExtensions
{
    public static UriParameterCollection Add(this UriParameterCollection builder, IEnumerable<KeyValuePair<string?, string?>> parameters)
    {
        Check.NotNull(parameters);
        foreach (var (key, value) in parameters)
        {
            builder.Add(key, value);
        }
        return builder;
    }
}