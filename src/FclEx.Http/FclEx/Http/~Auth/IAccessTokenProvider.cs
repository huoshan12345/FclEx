namespace FclEx.Http;

public interface IAccessTokenProvider
{
    Task<string> GetTokenAsync(string[] scopes, bool forceRefresh = false);
}

public static class AccessTokenProviderExtensions
{
    public static Task<string> GetTokenAsync(this IAccessTokenProvider provider, string scope, bool forceRefresh = false)
    {
        return provider.GetTokenAsync([scope], forceRefresh);
    }
}