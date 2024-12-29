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
    public async Task IHaveANupkgFileFor(string packageName, string packageVersion)
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
    [When(@"^I publish the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task IPublishThePackage(string packageName, string packageVersion)
    {
        var content = new MultipartFormDataContent();

        var packageFile = this._packageContext.AvailableLocalPackages[new Tuple<string, string>(packageName, packageVersion)];
        var package = new StreamContent(File.OpenRead(packageFile));

        content.Add(package, "package", Path.GetFileName(packageFile));

        var req = new HttpRequestMessage
        {
            RequestUri = new Uri("/api/v2/package", UriKind.Relative),
            Method = HttpMethod.Put,
            Content = content
        };

        var resp = await this._httpContext.HttpClient.SendAsync(req);

        this._httpContext.Response.StatusCode = resp.StatusCode;
        this._httpContext.Response.Headers = resp.Headers;
        this._httpContext.Response.Content = resp.Content;
    }

    [When(@"^I get the list of versions for the package ([a-zA-Z.]+)$")]
    public async Task IGetTheListOfVersionsForThePackage(string packageName)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"/v3-flatcontainer/{packageName.ToLowerInvariant()}/index.json", UriKind.Relative),
            Method = HttpMethod.Get
        };

        var resp = await this._httpContext.HttpClient.SendAsync(req);

        this._httpContext.Response.StatusCode = resp.StatusCode;
        this._httpContext.Response.Headers = resp.Headers;
        this._httpContext.Response.Content = resp.Content;

        var versions = await resp.Content.ReadFromJsonAsync<GetPackageVersionsResponse>();
        this._packageContext.VersionList = versions?.Versions;
    }

    [Then(@"^The list of versions should contain ([a-zA-Z0-9.]+)$")]
    public void TheListOfVersionsShouldContain(string packageVersion)
    {
        this._packageContext.VersionList.Should().NotBeNull();
        this._packageContext.VersionList.Should().Contain(packageVersion);
    }

    [When(@"^I download the (\.nu(?:pkg|spec)) file for the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task IDownloadThePackage(string extension, string packageName, string packageVersion)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"/v3-flatcontainer/{packageName.ToLowerInvariant()}/{packageVersion.ToLowerInvariant()}/{packageName.ToLowerInvariant()}.{packageVersion.ToLowerInvariant()}{extension}", UriKind.Relative),
            Method = HttpMethod.Get,
        };

        var resp = await this._httpContext.HttpClient.SendAsync(req);

        this._httpContext.Response.StatusCode = resp.StatusCode;
        this._httpContext.Response.Headers = resp.Headers;
        this._httpContext.Response.Content = resp.Content;
    }
}
