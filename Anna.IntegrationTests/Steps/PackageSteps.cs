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
    private readonly AnnaClientContext _clientContext;
    private readonly PackageContext _packageContext;

    public PackageSteps(AnnaClientContext clientContext, PackageContext packageContext)
    {
        this._clientContext = clientContext;
        this._packageContext = packageContext;
    }

    private record PackageVersion(string Version);

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

    [Given("I have uploaded the following packages")]
    public async Task GivenIHaveUploadedTheFollowingPackages(DataTable table)
    {
        var packages = table.CreateSet<NuGetPackage>();
        foreach (var package in packages)
        {
            await this.MakePackageAvailable(package.Id, package.Version);
            await this.UploadPackage(package.Id, package.Version);
        }
    }

    [Then(@"^I can upload the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task ThenICanUploadThePackage(string packageName, string packageVersion)
    {
        await this.UploadPackage(packageName, packageVersion);
    }

    [Then(@"^I can download the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task ThenICanDownloadThePackage(string packageName, string packageVersion)
    {
        _ = await this._clientContext.AnnaClient.DownloadPackage(packageName, packageVersion);
    }

    [Then(@"^I can download the package spec for ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task ThenICanDownloadThePackageSpecFor(string packageName, string packageVersion)
    {
        _ = await this._clientContext.AnnaClient.DownloadPackageSpec(packageName, packageVersion);
    }

    [Given(@"^I have unlisted the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    [Then(@"^I can unlist the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task ThenICanUnlistThePackage(string packageName, string packageVersion)
    {
        await this._clientContext.AnnaClient.DeletePackage(packageName, packageVersion);
    }

    [When("^I get a list of package versions for ([a-zA-Z.]+)")]
    public async Task WhenIGetAListOfPackageVersions(string packageName)
    {
        this._packageContext.VersionList = (await this._clientContext.AnnaClient.GetVersions(packageName)).Versions;
    }

    [Then(@"^I can relist the package ([a-zA-Z.]+)@([a-zA-Z0-9.]+)$")]
    public async Task ThenICanRelistThePackage(string packageName, string packageVersion)
    {
        await this._clientContext.AnnaClient.RestorePackage(packageName, packageVersion);
    }

    [Then(@"^the list of package versions should contain (\d+) item(?:s)?$")]
    public void ThenTheListOfPackageVersionsShouldContain(int expectedCount)
    {
        this._packageContext.VersionList.ShouldNotBeNull();
        this._packageContext.VersionList.Count.ShouldBe(expectedCount);
    }
    
    [Then("the list of package versions should contain the following versions")]
    public void ThenTheListOfPackageVersionsShouldContainTheFollowingVersions(DataTable table)
    {
        this._packageContext.VersionList.ShouldNotBeNull();
        
        var versions = table.CreateSet<PackageVersion>();
        foreach (var version in versions)
        {
            this._packageContext.VersionList.ShouldContain(version.Version);
        }
    }


    private async Task MakePackageAvailable(string packageName, string packageVersion)
    {
        if (this._packageContext.AvailableLocalPackages.ContainsKey(
            new Tuple<string, string>(packageName, packageVersion)))
        {
            return;
        }

        var httpClient = new HttpClient();
        var req = new HttpRequestMessage
        {
            RequestUri =
                new Uri(
                $"https://globalcdn.nuget.org/packages/{packageName.ToLowerInvariant()}.{packageVersion.ToLowerInvariant()}.nupkg?packageVersion={packageVersion.ToLowerInvariant()}",
                UriKind.Absolute),
            Method = HttpMethod.Get
        };

        var resp = await httpClient.SendAsync(req);

        resp.StatusCode.ShouldBe(HttpStatusCode.OK);

        var packagePath = Path.GetTempFileName();
        await using var fileStream = File.Open(packagePath, FileMode.Open);

        await resp.Content.CopyToAsync(fileStream);

        this._packageContext.AvailableLocalPackages.Add(new Tuple<string, string>(packageName, packageVersion),
                                                        packagePath);
    }

    private async Task UploadPackage(string packageName, string packageVersion)
    {
        if (this._packageContext.AvailableRemotePackages.Contains(
            new Tuple<string, string>(packageName, packageVersion)))
        {
            return;
        }

        var packageFile =
            this._packageContext.AvailableLocalPackages[new Tuple<string, string>(packageName, packageVersion)];
        await this._clientContext.AnnaClient.UploadPackage(File.OpenRead(packageFile));
        this._packageContext.AvailableRemotePackages.Add(new Tuple<string, string>(packageName, packageVersion));
        
        Thread.Sleep(1000);
    }

    private record NuGetPackage(string Id, string Version);
}
