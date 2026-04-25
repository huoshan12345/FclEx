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
}
