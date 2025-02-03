using System.Text.Json.Serialization;

namespace Anna.Api.Models.RegistrationIndex;

public class CatalogEntryDto
{
    [JsonPropertyName("@id")]
    public required string Id { get; init; }

    [JsonPropertyName("id")]
    public required string PackageId { get; init; }

    public required string Version { get; init; }
}
