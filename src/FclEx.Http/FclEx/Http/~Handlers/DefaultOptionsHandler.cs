#if NET5_0_OR_GREATER
namespace FclEx.Http;

/// <summary>
/// Applies default <see cref="HttpRequestOptions"/> to each outgoing request message.
/// </summary>
/// <remarks>
/// Existing request option values with the same key are overwritten before the request is sent to the inner handler.
/// </remarks>
public class DefaultOptionsHandler : DelegatingHandler
{
    /// <summary>
    /// The options that will be copied to each request.
    /// </summary>
    public readonly HttpRequestOptions Options = [];

    /// <summary>
    /// Stores or replaces an option that should be applied to future requests.
    /// </summary>
    /// <typeparam name="TValue">The option value type.</typeparam>
    /// <param name="key">The request option key.</param>
    /// <param name="value">The value to set for the key.</param>
    /// <returns>The same handler instance for configuration chaining.</returns>
    public DefaultOptionsHandler SetOption<TValue>(HttpRequestOptionsKey<TValue> key, TValue value)
    {
        Options.Set(key, value);
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var (key, value) in Options)
        {
            request.Options.Set(key, value);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
#endif
