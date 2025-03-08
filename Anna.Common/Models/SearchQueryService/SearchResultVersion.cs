using System.Text.Json.Serialization;

namespace Anna.Common.Models.SearchQueryService;

public class SearchResultVersion
{
    [JsonPropertyName("@id")]
    public required string Id { get; set; }
    public required string Version { get; set; }
    public required int Downloads { get; set; }
}
