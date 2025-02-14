namespace FclEx.Http;

public class SimpleCookie
{
    public SimpleCookie() { }

    public SimpleCookie(string name, string value, string domain, string? path = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        Path = path ?? "/";
    }

    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    public Cookie ToCookie()
    {
        return new Cookie(Name, Value, Path, Domain);
    }
}