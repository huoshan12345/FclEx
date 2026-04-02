namespace FclEx.Http;

public class ClientCredentialsTokenProviderOptions
{
    public required string Authority { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}