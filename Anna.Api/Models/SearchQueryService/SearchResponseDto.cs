using System.Collections.Generic;

namespace Anna.Api.Models.SearchQueryService;

public class SearchResponseDto
{
    public required int TotalHits { get; set; }
    public required List<SearchResultDto> Results { get; set; }
}
