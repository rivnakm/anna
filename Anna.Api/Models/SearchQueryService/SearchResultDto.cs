using System.Collections.Generic;

namespace Anna.Api.Models.SearchQueryService;

public class SearchResultDto
{
    public required string Id { get; set; }
    public required string Version { get; set; }
    public required List<SearchResultVersionDto> Versions { get; set; }
    public required List<SearchResultPackageTypeDto> PackageTypes { get; set; }
}
