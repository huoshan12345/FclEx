#if NET5_0_OR_GREATER
namespace FclEx.Http;

public class DefaultOptionsHandler : DelegatingHandler
{
    public readonly HttpRequestOptions Options = [];

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