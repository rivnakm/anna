using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Anna.Api.Models;

public class IndexDto
{
    public required string Version { get; init; }
    public required List<Resource> Resources { get; init; }

    public class Resource
    {
        [JsonPropertyName("@id")]
        public required string Id { get; init; }

        [JsonPropertyName("@type")]
        public required string Type { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Comment { get; init; }
    }
}
