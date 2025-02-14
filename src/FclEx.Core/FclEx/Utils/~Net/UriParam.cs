namespace FclEx.Utils;

/// <summary>
/// Represents a single URI parameter as a key-value pair.
/// This is an immutable data structure.
/// </summary>
public readonly record struct UriParam : IRenderable
{
    private readonly string? _key;
    private readonly string? _value;

    /// <summary>
    /// Represents a single URI parameter as a key-value pair.
    /// This is an immutable data structure.
    /// </summary>
    /// <param name="key">The key of the URI parameter.</param>
    /// <param name="value">The value of the URI parameter.</param>
    public UriParam(string? key, string? value)
    {
        _key = key;
        _value = value;
    }

    /// <summary>The key of the URI parameter.</summary>
    public string Key => _key ?? "";

    /// <summary>The value of the URI parameter.</summary>
    public string Value => _value ?? "";

    public void Render(StringBuilder builder)
    {
        // see https://source.dot.net/#System.Web.HttpUtility/System/Web/HttpUtility.cs,e8f7afaff17514d9,references
        if (Key.IsNotEmpty())
        {
            builder.Append(HttpUtility.UrlEncode(Key));
            builder.Append('=');
        }

        if (Value.IsNotEmpty())
        {
            builder.Append(HttpUtility.UrlEncode(Value));
        }
    }

    public override string ToString()
    {
        return this.RenderToString();
    }

    public KeyValuePair<string, string> ToKeyValuePair() => KeyValuePair.Create(Key, Value);

    public static UriParam From(KeyValuePair<string, string> pair) => new(pair.Key, pair.Value);
    public static UriParam From((string, string) pair) => new(pair.Item1, pair.Item2);

    public static implicit operator UriParam((string, string) pair) => From(pair);
    public static implicit operator UriParam(KeyValuePair<string, string> pair) => From(pair);
    public static implicit operator KeyValuePair<string, string>(UriParam param) => param.ToKeyValuePair();

    public void Deconstruct(out string key, out string value)
    {
        key = Key;
        value = Value;
    }
}