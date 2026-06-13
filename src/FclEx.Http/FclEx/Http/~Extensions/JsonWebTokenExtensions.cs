using Duende.IdentityModel;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FclEx.Http;

/// <summary>
/// Extensions for reading OAuth/OIDC data from JSON web tokens.
/// </summary>
public static class JsonWebTokenExtensions
{
    /// <summary>
    /// Returns all scope values from <c>scope</c> claims.
    /// Claim values containing space-separated scopes are split into individual entries.
    /// </summary>
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
