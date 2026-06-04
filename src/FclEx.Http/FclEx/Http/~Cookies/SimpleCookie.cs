namespace FclEx.Http;

public class SimpleCookie
{
    public SimpleCookie() { }

    public SimpleCookie(string name, string? value, string? path, string? domain)
    {
        Name = Check.NotNull(name);
        Value = value ?? "";
        Domain = domain ?? "";
        Path = path ?? "/";
    }

    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Domain { get; set; } = "";
    public string Path { get; set; } = "/";
}