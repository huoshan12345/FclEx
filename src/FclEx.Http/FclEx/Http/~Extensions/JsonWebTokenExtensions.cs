using Duende.IdentityModel;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FclEx.Http;

public static class JsonWebTokenExtensions
{
    public static List<string> GetScopes(this JsonWebToken token)
    {
        var scopes = new List<string>(0);
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var claim in token.Claims.Where(m => m.Type == JwtClaimTypes.Scope))
        {
            foreach (var scope in claim.Value.AsSpan().EnumerateSplit([' ']))
            {
                scopes.Add(scope.ToString());
            }
        }

        return scopes;
    }
}