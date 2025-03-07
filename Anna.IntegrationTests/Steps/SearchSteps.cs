using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Anna.Api.Models.SearchQueryService;
using Anna.IntegrationTests.Contexts;
using Microsoft.AspNetCore.Http.Extensions;
using Reqnroll;
using Shouldly;

namespace Anna.IntegrationTests.Steps;

[Binding]
public class SearchSteps
{
    private readonly HttpContext _httpContext;
    private readonly SearchContext _searchContext;

    public SearchSteps(HttpContext httpContext, SearchContext searchContext)
    {
        this._httpContext = httpContext;
        this._searchContext = searchContext;
    }

    [Given(@"I have a search query:\s*(.*)$")]
    public void GivenIHaveASearchQuery(string query)
    {
        this._searchContext.QueryBuilder = new QueryBuilder();
        this._searchContext.QueryBuilder.Add("q", query);
    }

    [Given("^I set the search parameter (.*) to (.*)$")]
    private void GivenISetTheSearchParameterTo(string key, string value)
    {
        this._searchContext.QueryBuilder!.Add(key, value);
    }

    [When(@"^I perform a search with query:\s*(.*)$")]
    public async Task WhenIPerformASearchWithQuery(string query)
    {
        var queryBuilder = new QueryBuilder
        {
            {
                "q", query
            }
        };
        var req = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/searchqueryservice/v3.5{queryBuilder}", UriKind.Relative)
        };

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);

        if (this._httpContext.Response.IsSuccessStatusCode)
        {
            this._searchContext.SearchResponse = await this._httpContext.Response.Content.ReadFromJsonAsync<SearchResponseDto>();
        }
    }
    
    [When("I perform the search")]
    public async Task WhenIPerformTheSearch()
    {
        var req = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"/searchqueryservice/v3.5{this._searchContext.QueryBuilder}", UriKind.Relative)
        };

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);

        if (this._httpContext.Response.IsSuccessStatusCode)
        {
            this._searchContext.SearchResponse = await this._httpContext.Response.Content.ReadFromJsonAsync<SearchResponseDto>();
        }
    }

    [Then(@"^the search results should contain (\d+) item(?:s)? and (\d+) total hit(?:s)?$")]
    public void ThenTheSearchResultsShouldContainDItemSAndDTotalHitS(int count, int totalHits)
    {
        this._searchContext.SearchResponse.ShouldNotBeNull();
        this._searchContext.SearchResponse.Results.Count.ShouldBe(count);
        this._searchContext.SearchResponse.TotalHits.ShouldBe(totalHits);
    }

    [Then(@"^the search result for (.*) should contain (\d+) version(?:s)?$")]
    public void ThenTheSearchResultForShouldContainDVersionS(string packageName, int versions)
    {
        this._searchContext.SearchResponse.ShouldNotBeNull();
        var package = this._searchContext.SearchResponse.Results.SingleOrDefault(r => r.Id == packageName);
        package.ShouldNotBeNull();
        
        package.Versions.Count.ShouldBe(versions);
    }
}
