using System.Security.Claims;

namespace FclEx.Http.Extensions;

public class JsonWebTokenExtensionsTests
{
    [Fact]
    public void GetScopes_WhenScopeClaimContainsSpaceSeparatedValues_ReturnsEachScope()
    {
        var token = CreateToken(
            new Claim(JwtClaimTypes.Scope, "read write"),
            new Claim(JwtClaimTypes.Scope, "admin"));

        var scopes = token.GetScopes();

        Assert.Equal(["read", "write", "admin"], scopes);
    }

    [Fact]
    public void GetScopes_WhenTokenHasNoScopeClaim_ReturnsEmptyList()
    {
        var token = CreateToken(new Claim(JwtClaimTypes.Name, "alice"));

        var scopes = token.GetScopes();

        Assert.Empty(scopes);
    }

    private static JsonWebToken CreateToken(params Claim[] claims)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
        };

        var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
        return new JsonWebToken(handler.CreateToken(descriptor));
    }
}
