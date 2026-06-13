namespace FclEx.Http;

/// <summary>
/// Configures OAuth/OIDC client-credentials token acquisition.
/// </summary>
public class ClientCredentialsTokenProviderOptions
{
    /// <summary>
    /// The authority URL used for discovery document lookup.
    /// </summary>
    public string Authority { get; set; } = "";

    /// <summary>
    /// The client identifier sent to the token endpoint.
    /// </summary>
    public string ClientId { get; set; } = "";

    /// <summary>
    /// The client secret sent to the token endpoint.
    /// </summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// Discovery validation rules passed to Duende IdentityModel. By default, key-set validation is disabled.
    /// </summary>
    public DiscoveryPolicy Policy { get; set; } = new() { RequireKeySet = false };
}
