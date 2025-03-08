using System.Collections.Generic;

namespace Anna.Common.Models.SearchQueryService;

public class SearchResponse
{
    public required int TotalHits { get; set; }
    public required List<SearchResult> Results { get; set; }
}
