using FclEx.Json;
using FclEx.NewtonsoftJson;

namespace FclEx.Http.Tests.Models;

/// <summary>
/// Model to test APIs on mockapi.io
/// </summary>
public class MockApiModel
{
    [JsonProperty("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("avatar")]
    public required string Avatar { get; set; }

    [JsonProperty("id")]
    [JsonConverter(typeof(WriteAsStringConverter))]
    public int Id { get; set; }
}