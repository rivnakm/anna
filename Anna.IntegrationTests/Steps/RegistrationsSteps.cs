using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Anna.Api.Models.RegistrationIndex;
using Anna.IntegrationTests.Contexts;
using Reqnroll;
using Shouldly;

namespace Anna.IntegrationTests.Steps;

[Binding]
public class RegistrationsSteps
{
    private readonly HttpContext _httpContext;
    private readonly RegistrationsContext _registrationsContext;

    public RegistrationsSteps(HttpContext httpContext, RegistrationsContext registrationsContext)
    {
        this._httpContext = httpContext;
        this._registrationsContext = registrationsContext;
    }

    [When(@"^I get the registration index for the package ([a-zA-Z.]+)$")]
    public async Task WhenIGetTheRegistrationIndexForThePackage(string packageName)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"/registrationbaseurl/v3.6/{packageName.ToLowerInvariant()}/index.json", UriKind.Relative),
            Method = HttpMethod.Get
        };

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);

        if (this._httpContext.Response.IsSuccessStatusCode)
        {
            this._registrationsContext.RegistrationIndex = await this._httpContext.Response.Content.ReadFromJsonAsync<RegistrationIndexDto>();
        }
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
        index.Items[pageNum-1].ShouldNotBeNull();
        index.Items[pageNum-1].Items!.ShouldContain(l => l.CatalogEntry.Version.ToString() == version);
    }
}
