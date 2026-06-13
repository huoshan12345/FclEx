namespace FclEx.Http;

/// <summary>
/// A serializable cookie DTO containing only name, value, domain, and path.
/// </summary>
/// <remarks>
/// This type is useful for persisting cookies to JSON without the runtime state carried by <see cref="Cookie"/>.
/// </remarks>
public class SimpleCookie
{
    /// <summary>
    /// Initializes an empty cookie with root path.
    /// </summary>
    public SimpleCookie() { }

    /// <summary>
    /// Initializes a cookie DTO and normalizes optional values.
    /// </summary>
    /// <param name="name">The cookie name. It cannot be null.</param>
    /// <param name="value">The cookie value. <see langword="null"/> becomes an empty string.</param>
    /// <param name="path">The cookie path. <see langword="null"/> becomes <c>/</c>.</param>
    /// <param name="domain">The cookie domain. <see langword="null"/> becomes an empty string.</param>
    public SimpleCookie(string name, string? value, string? path, string? domain)
    {
        Name = Check.NotNull(name);
        Value = value ?? "";
        Domain = domain ?? "";
        Path = path ?? "/";
    }

    /// <summary>
    /// The cookie name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The cookie value.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// The cookie domain, or an empty string when no domain is stored.
    /// </summary>
    public string Domain { get; set; } = "";

    /// <summary>
    /// The cookie path. The default is <c>/</c>.
    /// </summary>
    public string Path { get; set; } = "/";
}
