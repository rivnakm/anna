using Anna.Api.Models.SearchQueryService;
using Microsoft.AspNetCore.Http.Extensions;

namespace Anna.IntegrationTests.Contexts;

public class SearchContext
{
    public SearchResponseDto? SearchResponse { get; set; }
    public QueryBuilder? QueryBuilder { get; set; }
}
