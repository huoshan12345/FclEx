using System;
using System.Net;

namespace FclEx.Http;

public class SimpleCookie
{
    public SimpleCookie() { }

    public SimpleCookie(string name, string value, string domain)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Domain = domain ?? throw new ArgumentNullException(nameof(domain));
    }

    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;

    public Cookie ToCookie()
    {
        return new Cookie(Name, Value, "/", Domain);
    }
}