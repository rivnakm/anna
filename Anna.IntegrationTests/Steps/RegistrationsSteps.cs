using System.Threading.Tasks;
using Anna.IntegrationTests.Contexts;
using Reqnroll;
using Shouldly;

namespace Anna.IntegrationTests.Steps;

[Binding]
public class RegistrationsSteps
{
    private readonly AnnaClientContext _clientContext;
    private readonly RegistrationsContext _registrationsContext;

    public RegistrationsSteps(AnnaClientContext clientContext, RegistrationsContext registrationsContext)
    {
        this._clientContext = clientContext;
        this._registrationsContext = registrationsContext;
    }

    [When(@"^I get the registration index for the package ([a-zA-Z.]+)$")]
    public async Task WhenIGetTheRegistrationIndexForThePackage(string packageName)
    {
        this._registrationsContext.RegistrationIndex = await this._clientContext.AnnaClient.GetRegistrationIndex(packageName);
    }

    [Then(@"the registration index should contain (\d+) page(?:s)?")]
    public void TheRegistrationIndexShouldContainNPage(int numPages)
    {
        var index = this._registrationsContext.RegistrationIndex;

        index.ShouldNotBeNull();
        index.Count.ShouldBe(numPages);
    }

    [Then(@"the (\d+)(?:st|nd|th) inline registration index page should contain the version ([a-zA-Z0-9.]+)?")]
    public void TheNthInlineRegistrationIndexPageShouldContainTheVersion(int pageNum, string version)
    {
        var index = this._registrationsContext.RegistrationIndex;

        index.ShouldNotBeNull();
        index.Items[pageNum - 1].ShouldNotBeNull();
        index.Items[pageNum - 1].Items!.ShouldContain(l => l.CatalogEntry.Version.ToString() == version);
    }
}
