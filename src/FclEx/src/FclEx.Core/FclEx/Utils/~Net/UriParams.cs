using Microsoft.Collections.Extensions;

namespace FclEx.Utils;

public sealed class UriParams : IReadOnlyCollection<UriParam>, IRenderable
{
    private readonly MultiValueDictionary<string, string> _entries;

    public UriParams(string? query = null)
    {
        var dic = HttpUtility.ParseQueryString(query ?? "");
        _entries = dic.Enumerate().ToMultiValueDictionary();
    }

    public UriParams(IEnumerable<UriParam> parameters)
    {
        _entries = parameters.ToMultiValueDictionary(m => m.Key, m => m.Value);
    }

    public UriParams(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        _entries = parameters.ToMultiValueDictionary(m => m.Key, m => m.Value);
    }

    public override string ToString()
    {
        return this.RenderToString();
    }

    public void Render(StringBuilder builder)
    {
        foreach (var (key, values) in _entries)
        {
            // don't support empty key
            if (key.IsNullOrEmpty())
                continue;

            foreach (var value in values)
            {
                builder.Append(HttpUtility.UrlEncode(key));
                builder.Append('=');
                if (value.IsNotEmpty())
                {
                    builder.Append(HttpUtility.UrlEncode(value));
                }
                builder.Append('&');
            }
        }

        if (builder.Length > 0)
            builder.Length--; // remove the trailing extra &
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<UriParam> GetEnumerator()
    {
        return _entries.SelectMany(m => m.Value, (m, n) => new UriParam(m.Key, n)).GetEnumerator();
    }

    public UriParams Add(string key, string? value)
    {
        Check.NotEmpty(key);
        _entries.Add(key, value ?? "");
        return this;
    }

    public UriParams Set(string key, string? value)
    {
        Check.NotEmpty(key);
        _entries.Remove(key);
        return Add(key, value);
    }

    public UriParams Remove(string key)
    {
        Check.NotEmpty(key);
        _entries.Remove(key);
        return this;
    }

    public string? Get(string key)
    {
        return GetValues(key).LastOrDefault();
    }

    public IReadOnlyCollection<string> GetValues(string key)
    {
        Check.NotEmpty(key);
        return _entries.Get(key);
    }

    public bool TryGet(string key, [NotNullWhen(true)] out string? value)
    {
        Check.NotEmpty(key);

        if (TryGetValues(key, out var values))
        {
            value = values.Last();
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetValues(string key, [NotNullWhen(true)] out IReadOnlyCollection<string>? values)
    {
        return _entries.TryGetValue(key, out values);
    }

    public int Count => _entries.Count;

    public static UriParams Parse(string? query) => new(query);

    public static UriParams From(IEnumerable<KeyValuePair<string, string>> pairs) => new(pairs);
}

public static class UriParameterCollectionExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> AsKeyValuePairs(this UriParams collection)
    {
        return collection.Select(m => m.ToKeyValuePair());
    }

    public static UriParams Add(this UriParams collection, IEnumerable<KeyValuePair<string, string>> parameters)
    {
        Check.NotNull(parameters);
        foreach (var (key, value) in parameters)
        {
            collection.Add(key, value);
        }
        return collection;
    }

    public static UriParams Add(this UriParams collection, UriParam parameter)
    {
        return collection.Add(parameter.Key, parameter.Value);
    }
}