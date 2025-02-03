using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Anna.IntegrationTests.Contexts;
using Reqnroll;
using Shouldly;

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
        await this.MakePackageAvailable(packageName, packageVersion);
    }

    [Given(@"^I have uploaded the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task GivenIHaveUploadedThePackage(string packageName, string packageVersion)
    {
        await this.MakePackageAvailable(packageName, packageVersion);
        await this.UploadPackage(packageName, packageVersion);
    }

    [When(@"^I upload the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task WhenIUploadThePackage(string packageName, string packageVersion)
    {
        await this.UploadPackage(packageName, packageVersion);
    }

    [When(@"^I download the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task WhenIDownloadThePackage(string packageName, string packageVersion)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"/packagebaseaddress/v3/{packageName.ToLowerInvariant()}/{packageVersion.ToLowerInvariant()}/{packageName.ToLowerInvariant()}.{packageVersion.ToLowerInvariant()}.nupkg", UriKind.Relative),
            Method = HttpMethod.Get
        };

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);
    }

    [When(@"^I download the package spec for ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task WhenIDownloadThePackageSpecFor(string packageName, string packageVersion)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"/packagebaseaddress/v3/{packageName.ToLowerInvariant()}/{packageVersion.ToLowerInvariant()}/{packageName.ToLowerInvariant()}.{packageVersion.ToLowerInvariant()}.nuspec", UriKind.Relative),
            Method = HttpMethod.Get
        };

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);
    }

    [Given(@"^I have unlisted the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    [When(@"^I unlist the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task WhenIUnlistThePackage(string packageName, string packageVersion)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"/packagepublish/v2/{packageName}/{packageVersion}", UriKind.Relative),
            Method = HttpMethod.Delete
        };

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);
    }

    [When(@"^I relist the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task WhenIRelistThePackage(string packageName, string packageVersion)
    {
        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"/packagepublish/v2/{packageName}/{packageVersion}", UriKind.Relative),
            Method = HttpMethod.Post
        };

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);
    }

    private async Task MakePackageAvailable(string packageName, string packageVersion)
    {
        if (this._packageContext.AvailableLocalPackages.ContainsKey(new Tuple<string, string>(packageName, packageVersion)))
        {
            return;
        }

        var req = new HttpRequestMessage
        {
            RequestUri = new Uri($"https://globalcdn.nuget.org/packages/{packageName.ToLowerInvariant()}.{packageVersion.ToLowerInvariant()}.nupkg?packageVersion={packageVersion.ToLowerInvariant()}", UriKind.Absolute),
            Method = HttpMethod.Get
        };

        var resp = await this._httpContext.HttpClient.SendAsync(req);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);

        var packagePath = Path.GetTempFileName();
        await using var fileStream = File.Open(packagePath, FileMode.Open);

        await resp.Content.CopyToAsync(fileStream);

        this._packageContext.AvailableLocalPackages.Add(new Tuple<string, string>(packageName, packageVersion), packagePath);
    }

    private async Task UploadPackage(string packageName, string packageVersion)
    {
        if (this._packageContext.AvailableRemotePackages.Contains(new Tuple<string, string>(packageName, packageVersion)))
        {
            this._httpContext.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Accepted };
            return;
        }

        var content = new MultipartFormDataContent();

        var packageFile = this._packageContext.AvailableLocalPackages[new Tuple<string, string>(packageName, packageVersion)];
        var package = new StreamContent(File.OpenRead(packageFile));

        content.Add(package, "package", Path.GetFileName(packageFile));

        this._httpContext.Request.Content = content;

        this._httpContext.Request.RequestUri = new Uri("/packagepublish/v2", UriKind.Relative);
        this._httpContext.Request.Method = HttpMethod.Put;
        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(this._httpContext.Request);

        if (this._httpContext.Response.IsSuccessStatusCode)
        {
            this._packageContext.AvailableRemotePackages.Add(new Tuple<string, string>(packageName, packageVersion));
        }
        Thread.Sleep(1000);
    }
}
