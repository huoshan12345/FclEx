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

    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Domain { get; set; } = "";
    public string Path { get; set; } = "";
}