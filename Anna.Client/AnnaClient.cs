using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Anna.Client.Extensions;
using Anna.Client.Models;
using Anna.Common.Models;
using Anna.Common.Models.RegistrationIndex;
using Anna.Common.Models.SearchQueryService;
using Flurl;
using Microsoft.Extensions.Options;
using Index=Anna.Common.Models.Index;

namespace Anna.Client;

public class AnnaClient : IAnnaClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<AnnaClientOptions> _options;

    public AnnaClient(HttpClient httpClient, IOptions<AnnaClientOptions> options)
    {
        this._httpClient = httpClient;
        this._options = options;
    }

    public async Task<bool> CheckHealth()
    {
        var req = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = this._options.Value.IndexUrl.RemovePath().AppendPathSegment("/healthcheck").ToUri()
        };
        var resp = await this._httpClient.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }

    public async Task<Index> GetIndex()
    {
        var resp = await this._httpClient.GetAsync(this._options.Value.IndexUrl);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<Index>() ??
               throw new InvalidDataException("Unable to deserialize message");
    }
    
    public async Task<PackageVersions> GetVersions(string packageId)
    {
        packageId = packageId.ToLowerInvariant();
        var packageBaseUrl = await this.GetResourceUrl("PackageBaseAddress", "3.0.0");
        var packageUrl = packageBaseUrl.AppendPathSegments(packageId, "index.json");
        
        var resp = await this._httpClient.GetAsync(packageUrl);
        resp.EnsureSuccessStatusCode();
        
        return await resp.Content.ReadFromJsonAsync<PackageVersions>() ?? throw new InvalidDataException("Unable to deserialize message");
    }

    public async Task<DownloadPackageResponse> DownloadPackage(string packageId, string packageVersion)
    {
        return await this.DownloadPackage(packageId, packageVersion, "nupkg");
    }

    public async Task<DownloadPackageResponse> DownloadPackageSpec(string packageId, string packageVersion)
    {
        return await this.DownloadPackage(packageId, packageVersion, "nuspec");
    }

    public async Task UploadPackage(Stream package)
    {
        var content = new MultipartFormDataContent();
        var packageContent = new StreamContent(package);

        content.Add(packageContent, "package", $"{Guid.NewGuid()}.nupkg");

        var uploadUrl = await this.GetResourceUrl("PackagePublish", "2.0.0");
        var req = new HttpRequestMessage
        {
            Content = content,
            Method = HttpMethod.Put,
            RequestUri = uploadUrl
        };

        var resp = await this._httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeletePackage(string packageId, string packageVersion)
    {
        var deleteUrl = await this.GetResourceUrl("PackagePublish", "2.0.0");
        deleteUrl = deleteUrl.AppendPathSegments(packageId, packageVersion).ToUri();

        var resp = await this._httpClient.DeleteAsync(deleteUrl);
        resp.EnsureSuccessStatusCode();
    }

    public async Task RestorePackage(string packageId, string packageVersion)
    {
        var updateUrl = await this.GetResourceUrl("PackagePublish", "2.0.0");
        updateUrl = updateUrl.AppendPathSegments(packageId, packageVersion).ToUri();

        var resp = await this._httpClient.PostAsync(updateUrl, new StringContent(""));
        resp.EnsureSuccessStatusCode();
    }

    public async Task<RegistrationIndex> GetRegistrationIndex(string packageId)
    {
        var registrationUrl = await this.GetResourceUrl("RegistrationsBaseUrl", "3.6.0");
        registrationUrl = registrationUrl.AppendPathSegments(packageId.ToLowerInvariant(), "index.json").ToUri();

        var resp = await this._httpClient.GetAsync(registrationUrl);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<RegistrationIndex>() ?? throw new InvalidDataException("Unable to deserialize message");
    }

    public async Task<SearchResponse> Search(SearchRequest request)
    {
        var searchUrl = await this.GetResourceUrl("SearchQueryService", "3.5.0");
        var req = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = searchUrl.SetQueryParams(request.ToQueryParams()).ToUri()
        };

        var resp = await this._httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SearchResponse>() ??
               throw new InvalidDataException("Unable to deserialize message");
    }

    private async Task<Uri> GetResourceUrl(string resourceName, string resourceVersion)
    {
        var index = await this.GetIndex();
        return index.GetResourceUrl(resourceName, resourceVersion);
    }

    private async Task<DownloadPackageResponse> DownloadPackage(string packageId, string packageVersion, string extension)
    {
        packageId = packageId.ToLowerInvariant();
        packageVersion = packageVersion.ToLowerInvariant();
        var packageBaseUrl = await this.GetResourceUrl("PackageBaseAddress", "3.0.0");
        var packageUrl = packageBaseUrl.AppendPathSegments(packageId, packageVersion, $"{packageId}.{packageVersion}.{extension}");

        var resp = await this._httpClient.GetAsync(packageUrl);
        resp.EnsureSuccessStatusCode();
        if (resp.Content.Headers.ContentDisposition?.FileName == null)
        {
            throw new InvalidDataException("Response did not contain a filename");
        }

        return new DownloadPackageResponse(await resp.Content.ReadAsStreamAsync(), resp.Content.Headers.ContentDisposition.FileName);
    }
}
