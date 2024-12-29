using System.Net.Http.Json;
using System.Threading.Tasks;
using Anna.Api.Models;
using Anna.IntegrationTests.Contexts;
using FluentAssertions;
using Reqnroll;

[Binding]
public sealed class HttpSteps
{
    private readonly HttpContext _httpContext;
    private readonly IndexContext _indexContext;

    public HttpSteps(HttpContext httpContext, IndexContext indexContext)
    {
        this._httpContext = httpContext;
        this._indexContext = indexContext;
    }

    [Then(@"^The index should be version ([a-zA-Z0-9/.]+)$")]
    public async Task TheIndexShouldBeVersion(string indexVersion)
    {
        if (this._indexContext.Response is null)
        {
            this._indexContext.Response = await this._httpContext.Response.Content.ReadFromJsonAsync<GetIndexResponse>();
        }

        this._indexContext.Response.Version.Should().Be(indexVersion);
    }

    [Then(@"^The index should contain a ([a-zA-Z0-9/.]+) resource$")]
    public async Task TheIndexShouldContainAResource(string resourceType)
    {
        if (this._indexContext.Response is null)
        {
            this._indexContext.Response = await this._httpContext.Response.Content.ReadFromJsonAsync<GetIndexResponse>();
        }

        this._indexContext.Response.Resources.Should().Contain(r => r.Type == resourceType);
    }
}
