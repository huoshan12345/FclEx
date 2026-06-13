namespace FclEx.Web;

/// <summary>
/// Describes a form submission target and the name-value pairs to submit.
/// </summary>
/// <remarks>
/// This type is only a data container. It does not parse an HTML document, infer the form method,
/// or send the request. Callers can populate <see cref="Params"/> from a parsed form and then use
/// <see cref="SubmitUri"/> and <see cref="Method"/> to build the actual HTTP request.
/// </remarks>
/// <param name="submitUri">The URI to submit to, usually resolved from a form's action attribute.</param>
public class FormData(Uri submitUri)
{
    /// <summary>
    /// Gets or sets the URI to submit to.
    /// </summary>
    public Uri SubmitUri { get; set; } = submitUri;

    /// <summary>
    /// Gets or sets the HTTP method to use when submitting the parameters.
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Gets or sets the parameters to submit. Duplicate keys and keyless values are preserved by <see cref="UriParams"/>.
    /// </summary>
    public UriParams Params { get; set; } = [];
}
