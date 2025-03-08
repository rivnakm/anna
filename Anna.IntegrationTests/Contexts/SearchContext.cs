using Anna.Client.Models;
using Anna.Common.Models.SearchQueryService;
using Microsoft.AspNetCore.Http.Extensions;

namespace Anna.IntegrationTests.Contexts;

public class SearchContext
{
    public SearchResponse? SearchResponse { get; set; }
    public SearchRequest? SearchRequest { get; set; }
}
