namespace FclEx.Utils;

public readonly record struct UriParam(string Key, string Value) : IRenderable
{
    public void Render(StringBuilder builder)
    {
        if (Key.IsNullOrEmpty())
            return;

        builder.Append(HttpUtility.UrlEncode(Key));
        builder.Append('=');
        if (Value.IsNotEmpty())
        {
            builder.Append(HttpUtility.UrlEncode(Value));
        }
    }

    public KeyValuePair<string, string> ToKeyValuePair() => KeyValuePair.Create(Key, Value);

    public static UriParam From(KeyValuePair<string, string> pair) => new(pair.Key, pair.Value);
    public static UriParam From((string, string) pair) => new(pair.Item1, pair.Item2);

    public static implicit operator UriParam((string, string) pair) => From(pair);
    public static implicit operator UriParam(KeyValuePair<string, string> pair) => From(pair);
    public static implicit operator KeyValuePair<string, string>(UriParam param) => param.ToKeyValuePair();
}