using System.Text.Json.Serialization;

namespace Anna.Api.Models.SearchQueryService;

public class SearchResultVersionDto
{
    [JsonPropertyName("@id")]
    public required string Id { get; set; }
    public required string Version { get; set; }
    public required int Downloads { get; set; }
}
