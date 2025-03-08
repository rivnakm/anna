using System.Collections.Generic;

namespace Anna.Common.Models.SearchQueryService;

public class SearchResult
{
    public required string Id { get; set; }
    public required string Version { get; set; }
    public required List<SearchResultVersion> Versions { get; set; }
    public required List<SearchResultPackageType> PackageTypes { get; set; }
}
