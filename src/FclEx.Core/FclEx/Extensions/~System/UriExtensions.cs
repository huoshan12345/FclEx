namespace FclEx.Extensions;

public static class UriExtensions
{
    /// <summary>
    /// Resolves a relative URI against the current base URI.
    /// </summary>
    /// <param name="baseUri">The base absolute URI used for resolution.</param>
    /// <param name="relativeUri">The relative URI reference to be resolved against the base.</param>
    /// <returns>A new <see cref="Uri"/> representing the resolved absolute URI.</returns>
    public static Uri Resolve(this Uri baseUri, Uri relativeUri)
    {
        return new Uri(baseUri, relativeUri);
    }

    /// <summary>
    /// Resolves a relative URI against the current base URI.
    /// </summary>
    /// <param name="baseUri">The base absolute URI used for resolution.</param>
    /// <param name="relativeUri">The relative URI reference to be resolved against the base.</param>
    /// <returns>A new <see cref="Uri"/> representing the resolved absolute URI.</returns>
    public static Uri Resolve(this Uri baseUri, string relativeUri)
    {
        return new Uri(baseUri, relativeUri);
    }

    /// <summary>
    /// Returns a new URI with the specified path, replacing the existing one.
    /// </summary>
    /// <param name="uri">The source URI.</param>
    /// <param name="path">The new absolute path string (e.g., "/api/v1/users").</param>
    /// <returns>A new <see cref="Uri"/> instance with the updated path.</returns>
    /// <remarks>
    /// This method uses <see cref="UriBuilder"/> to perform a total replacement of the path.
    /// Other components such as Scheme, Host, Port, and Query strings are preserved.
    /// </remarks>
    public static Uri WithPath(this Uri uri, string path)
    {
        var builder = new UriBuilder(uri)
        {
            Path = path
        };
        return builder.Uri;
    }

    /// <summary>
    /// Gets the path part of the URI without the query string or fragment.
    /// </summary>
    /// <remarks>
    /// For absolute URIs, this returns <see cref="Uri.AbsolutePath"/>.<br/>
    /// For relative URIs, this removes the query string and fragment from
    /// <see cref="Uri.OriginalString"/>.
    /// </remarks>
    public static string GetPath(this Uri uri)
    {
        if (uri.IsAbsoluteUri)
            return uri.AbsolutePath;

        var text = uri.OriginalString;
        var endIndex = text.IndexOfAny(['?', '#']);

        return endIndex >= 0 ? text[..endIndex] : text;
    }

    extension(Uri)
    {
        public static Uri New([StringSyntax(StringSyntaxAttribute.Uri, nameof(uriKind))] string uriString, UriKind uriKind = UriKind.RelativeOrAbsolute)
        {
            return new Uri(uriString, uriKind);
        }

        public static Uri? TryNew([StringSyntax(StringSyntaxAttribute.Uri, nameof(uriKind))] string? uriString, UriKind uriKind = UriKind.RelativeOrAbsolute)
        {
            return uriString is null
                ? null
                : new Uri(uriString, uriKind);
        }
    }
}
