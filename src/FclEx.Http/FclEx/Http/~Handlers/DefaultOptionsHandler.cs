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
        var options = (IDictionary<string, object?>)request.Options;
        foreach (var option in Options)
        {
            options[option.Key] = option.Value;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
#endif