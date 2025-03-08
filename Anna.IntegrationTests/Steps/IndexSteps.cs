using System.Threading.Tasks;
using Anna.IntegrationTests.Contexts;
using Reqnroll;
using Shouldly;

namespace Anna.IntegrationTests.Steps;

[Binding]
public sealed class IndexSteps
{
    private readonly AnnaClientContext _clientContext;
    private readonly IndexContext _indexContext;

    public IndexSteps(AnnaClientContext clientContext, IndexContext indexContext)
    {
        this._clientContext = clientContext;
        this._indexContext = indexContext;
    }

    [When("I retrieve the index")]
    public async Task WhenIRetrieveTheIndex()
    {
        this._indexContext.Response = await this._clientContext.AnnaClient.GetIndex();
    }

    [Then(@"^The index should be version ([a-zA-Z0-9/.]+)$")]
    public void TheIndexShouldBeVersion(string indexVersion)
    {
        this._indexContext.Response.ShouldNotBeNull();
        this._indexContext.Response!.Version.ShouldBe(indexVersion);
    }

    [Then(@"^The index should contain a ([a-zA-Z0-9/.]+) resource$")]
    public void TheIndexShouldContainAResource(string resourceType)
    {
        this._indexContext.Response.ShouldNotBeNull();
        this._indexContext.Response!.Resources.ShouldContain(r => r.Type == resourceType);
    }
}
