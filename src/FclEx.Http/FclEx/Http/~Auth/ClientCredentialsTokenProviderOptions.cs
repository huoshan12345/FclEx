namespace FclEx.Http;

public class ClientCredentialsTokenProviderOptions
{
    public string Authority { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public DiscoveryPolicy Policy { get; set; } = new() { RequireKeySet = false };
}