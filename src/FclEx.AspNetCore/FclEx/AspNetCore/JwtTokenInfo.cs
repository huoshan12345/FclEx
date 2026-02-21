namespace FclEx.AspNetCore;

public class JwtTokenInfo
{
    public JwtTokenInfo(JwtSecurityToken token)
    {
        Token = Check.NotNull(token);
        ExpirationTime = token.ValidTo;
        IssuedAt = token.IssuedAt;
        AuthenticationTime = token.Payload.AuthTime is { } authTime
            ? DateTimeOffset.FromUnixTimeSeconds(authTime)
            : DateTimeOffset.MinValue;
        JwtId = token.Id;
        Issuer = token.Issuer;
        Audiences = token.Audiences.AsIReadOnlyList();
        Subject = token.Subject;
        Type = token.Header.Typ;
        AuthorizedParty = token.Payload.Azp;
        Scopes = token.Payload.Claims.FirstOrDefault(m => m.Type == JwtClaimTypes.Scope)?.Value.Split(' ') ?? [];
    }

    [LogPropertyIgnore]
    public JwtSecurityToken Token { get; }

    /// <summary>
    /// exp: Timestamp indicating when the token will expire.<br/>
    /// Value is Unix epoch time.
    /// </summary>
    public DateTimeOffset ExpirationTime { get; }

    /// <summary>
    /// iat: Timestamp indicating when the token was issued.<br/>
    /// Value is Unix epoch time.
    /// </summary>
    public DateTimeOffset IssuedAt { get; }

    /// <summary>
    /// auth_time: Timestamp indicating when the user was authenticated.<br/>
    /// Value is Unix epoch time.
    /// </summary>
    public DateTimeOffset AuthenticationTime { get; }

    /// <summary>
    /// jti: Unique identifier for the token. Useful for preventing token reuse (replay attacks).<br/>
    /// Value is a UUID.
    /// </summary>
    public string JwtId { get; }

    /// <summary>
    /// iss: Identifier of the entity that issued the token.<br/>
    /// This is often the URL of the identity provider or authorization server.
    /// </summary>
    public string Issuer { get; }

    /// <summary>
    /// aud: Intended recipient(s) of the token.
    /// </summary>
    public IReadOnlyList<string> Audiences { get; }

    /// <summary>
    /// sub: Identifier for the subject (user) of the token.<br/>
    /// Value is often a unique user ID.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// typ: Type of token. "Bearer" indicates this is a bearer token.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// azp: Client ID of the party to whom the token was issued.<br/>
    /// Value refers to the application or service that requested the token.
    /// </summary>
    public string AuthorizedParty { get; }

    /// <summary>
    /// scope: Space-separated list of granted scopes. Scopes define access permissions. 
    /// </summary>
    public string[] Scopes { get; }
}