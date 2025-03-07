using System.Net.Http.Json;
using System.Threading.Tasks;
using Anna.Api.Models;
using Anna.IntegrationTests.Contexts;
using Reqnroll;
using Shouldly;

namespace Anna.IntegrationTests.Steps;

[Binding]
public sealed class IndexSteps
{
    private readonly HttpContext _httpContext;
    private readonly IndexContext _indexContext;

    public IndexSteps(HttpContext httpContext, IndexContext indexContext)
    {
        this._httpContext = httpContext;
        this._indexContext = indexContext;
    }

    [Then(@"^The index should be version ([a-zA-Z0-9/.]+)$")]
    public async Task TheIndexShouldBeVersion(string indexVersion)
    {
        this._indexContext.Response ??= await this._httpContext.Response.Content.ReadFromJsonAsync<IndexDto>();

        this._indexContext.Response.ShouldNotBeNull();
        this._indexContext.Response!.Version.ShouldBe(indexVersion);
    }

    [Then(@"^The index should contain a ([a-zA-Z0-9/.]+) resource$")]
    public async Task TheIndexShouldContainAResource(string resourceType)
    {
        this._indexContext.Response ??= await this._httpContext.Response.Content.ReadFromJsonAsync<IndexDto>();

        this._indexContext.Response.ShouldNotBeNull();
        this._indexContext.Response!.Resources.ShouldContain(r => r.Type == resourceType);
    }
}
