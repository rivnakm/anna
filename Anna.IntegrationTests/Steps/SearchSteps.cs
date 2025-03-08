using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Anna.Client.Models;
using Anna.Common.Models.SearchQueryService;
using Anna.IntegrationTests.Contexts;
using Microsoft.AspNetCore.Http.Extensions;
using Reqnroll;
using Shouldly;

namespace Anna.IntegrationTests.Steps;

[Binding]
public class SearchSteps
{
    private readonly AnnaClientContext _clientContext;
    private readonly SearchContext _searchContext;

    public SearchSteps(AnnaClientContext clientContext, SearchContext searchContext)
    {
        this._clientContext = clientContext;
        this._searchContext = searchContext;
    }

    [Given(@"I have a search query:\s*(.*)$")]
    public void GivenIHaveASearchQuery(string query)
    {
        this._searchContext.SearchRequest = new SearchRequest
        {
            Query = query
        };
    }

    [Given("^I set the search to (include|exclude) prereleases$")]
    private void GivenISetTheSearchParameterTo(string includeExclude)
    {
        this._searchContext.SearchRequest!.Prerelease = includeExclude == "include";
    }

    [When(@"^I perform a search with query:\s*(.*)$")]
    public async Task WhenIPerformASearchWithQuery(string query)
    {
        this._searchContext.SearchResponse =
            await this._clientContext.AnnaClient.Search(new SearchRequest { Query = query });
    }

    [When("I perform the search")]
    public async Task WhenIPerformTheSearch()
    {
        this._searchContext.SearchRequest.ShouldNotBeNull();
        this._searchContext.SearchResponse =
            await this._clientContext.AnnaClient.Search(this._searchContext.SearchRequest);
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
