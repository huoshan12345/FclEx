namespace FclEx.Http.Tests.Models;

/// <summary>
/// Model to test APIs on mockapi.io
/// </summary>
public class MockApiModel
{
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("avatar")]
    public required string Avatar { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }
}