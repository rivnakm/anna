using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Anna.Api.Models;
using Anna.IntegrationTests.Contexts;
using FluentAssertions;
using Reqnroll;

namespace Anna.IntegrationTests.Steps;

[Binding]
public sealed class PackageSteps
{
    private readonly HttpContext _httpContext;
    private readonly PackageContext _packageContext;

    public PackageSteps(HttpContext httpContext, PackageContext packageContext)
    {
        this._httpContext = httpContext;
        this._packageContext = packageContext;
    }

    [Given(@"^I have a \.nupkg file for ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task GivenIHaveANupkgFileFor(string packageName, string packageVersion)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"https://globalcdn.nuget.org/packages/{packageName.ToLowerInvariant()}.{packageVersion.ToLowerInvariant()}.nupkg?packageVersion={packageVersion.ToLowerInvariant()}", UriKind.Absolute),
            Method = HttpMethod.Get
        };

        var resp = await this._httpContext.HttpClient.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var packagePath = Path.GetTempFileName();
        await using var fileStream = File.Open(packagePath, FileMode.Open);

        await resp.Content.CopyToAsync(fileStream);

        this._packageContext.AvailableLocalPackages.Add(new Tuple<string, string>(packageName, packageVersion), packagePath);
    }

    // TODO: test empty request content return 400
    [Given(@"^I set the request content to the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public void GivenISetTheRequestContentToThePackage(string packageName, string packageVersion)
    {
        var content = new MultipartFormDataContent();

        var packageFile = this._packageContext.AvailableLocalPackages[new Tuple<string, string>(packageName, packageVersion)];
        var package = new StreamContent(File.OpenRead(packageFile));

        content.Add(package, "package", Path.GetFileName(packageFile));

        this._httpContext.Request.Content = content;
    }

    [Then("The response should be a list of versions")]
    public async Task ThenTheResponseShouldBeAListOfVersions()
    {
        var versions = await this._httpContext.Response.Content.ReadFromJsonAsync<GetPackageVersionsResponse>();
        this._packageContext.VersionList = versions.Versions;
    }

    [Then(@"^The list of versions should ((?:not )?contain) ([a-zA-Z0-9.]+)$")]
    public void TheListOfVersionsShouldContain(string containOrNot, string packageVersion)
    {
        if (containOrNot == "contain")
        {
            this._packageContext.VersionList.Should().NotBeNull();
            this._packageContext.VersionList.Should().Contain(packageVersion);
        }
        else
        {
            if (this._packageContext.VersionList is not null)
            {
                this._packageContext.VersionList.Should().NotContain(packageVersion);
            }
        }
    }
}
