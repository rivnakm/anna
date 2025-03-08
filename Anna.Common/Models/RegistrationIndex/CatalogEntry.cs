using System.Text.Json.Serialization;

namespace Anna.Common.Models.RegistrationIndex;

public class CatalogEntry
{
    [JsonPropertyName("@id")]
    public required string Id { get; init; }

    [JsonPropertyName("id")]
    public required string PackageId { get; init; }

    public required string Version { get; init; }
}
