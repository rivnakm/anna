using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Anna.Client.Models;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Shouldly;

namespace Anna.Client.Test;

public class AnnaClientTest
{
    private readonly AnnaClient _annaClient;
    private readonly AnnaClientOptions _annaClientOptions;
    private readonly MockHttpMessageHandler _mockHttp;

    public AnnaClientTest()
    {
        this._mockHttp = new MockHttpMessageHandler();
        this._annaClientOptions = new AnnaClientOptions
        {
            IndexUrl = new Uri("http://localhost/v3/index.json")
        };

        this._annaClient = new AnnaClient(this._mockHttp.ToHttpClient(),
                                          new OptionsWrapper<AnnaClientOptions>(this._annaClientOptions));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, false)]
    public async Task TestCheckHealth(HttpStatusCode expectedStatusCode, bool expectedIsHealthy)
    {
        this._mockHttp.Expect("http://localhost/healthcheck").Respond(expectedStatusCode);

        (await this._annaClient.CheckHealth()).ShouldBe(expectedIsHealthy);
        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestGetIndex()
    {
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/search/v3.5\", \"@type\": \"SearchQueryService/3.5.0\"}]}");

        var response = await this._annaClient.GetIndex();

        response.ShouldNotBeNull();
        response.Version.ShouldBe("3.0.0");

        response.Resources.Count.ShouldBe(1);
        var resource = response.Resources[0];

        resource.ShouldNotBeNull();
        resource.Id.ShouldBe("http://localhost/search/v3.5");
        resource.Type.ShouldBe("SearchQueryService/3.5.0");

        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData("nupkg")]
    [InlineData("nuspec")]
    public async Task TestDownloadPackage(string extension)
    {
        const string id = "Azure.Core";
        const string lowerId = "azure.core";
        const string version = "1.45.0";
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/packagebaseaddress/v3\", \"@type\": \"PackageBaseAddress/3.0.0\"}]}");

        this._mockHttp.Expect($"http://localhost/packagebaseaddress/v3/{lowerId}/{version}/{lowerId}.{version}.{extension}")
            .Respond(_ => {
                var resp = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(new MemoryStream()),
                };
                resp.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"{lowerId}.{version}.{extension}"
                };
                return Task.FromResult(resp);
            });

        var package = (extension) switch
        {
            "nupkg" => await this._annaClient.DownloadPackage(id, version),
            "nuspec" => await this._annaClient.DownloadPackageSpec(id, version),
            _ => throw new ArgumentOutOfRangeException(nameof(extension), extension, null)
        };

        package.ShouldNotBeNull();
        package.FileName.ShouldBe($"{lowerId}.{version}.{extension}");

        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestUploadPackage()
    {
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/packagepublish/v2\", \"@type\": \"PackagePublish/2.0.0\"}]}");

        this._mockHttp.Expect("http://localhost/packagepublish/v2").With(m => m.Content is MultipartFormDataContent multipart && multipart.Count() == 1).Respond(HttpStatusCode.Accepted);

        await Should.NotThrowAsync(async () => await this._annaClient.UploadPackage(new MemoryStream()));

        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestDeletePackage()
    {
        const string id = "Azure.Core";
        const string version = "1.45.0";
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/packagepublish/v2\", \"@type\": \"PackagePublish/2.0.0\"}]}");

        this._mockHttp.Expect(HttpMethod.Delete, $"http://localhost/packagepublish/v2/{id}/{version}").Respond(HttpStatusCode.NoContent);

        await Should.NotThrowAsync(async () => await this._annaClient.DeletePackage(id, version));

        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestRestorePackage()
    {
        const string id = "Azure.Core";
        const string version = "1.45.0";
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/packagepublish/v2\", \"@type\": \"PackagePublish/2.0.0\"}]}");

        this._mockHttp.Expect(HttpMethod.Post, $"http://localhost/packagepublish/v2/{id}/{version}").Respond(HttpStatusCode.NoContent);

        await Should.NotThrowAsync(async () => await this._annaClient.RestorePackage(id, version));

        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestGetRegistrationIndex()
    {
        const string id = "Azure.Core";
        const string lowerId = "azure.core";
        const string version = "1.45.0";
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/registrationsbaseurl/v3.6\", \"@type\": \"RegistrationsBaseUrl/3.6.0\"}]}");

        const string registrationIndexJson = $$"""
                                               {
                                                   "count": 1,
                                                   "items": [
                                                       {
                                                           "@id": "http://localhost/registrationsbaseurl/v3.6/page/{{lowerId}}/{{version}}/index.json",
                                                           "count": 1,
                                                           "items": [
                                                               {
                                                                   "@id": "http://localhost/registrationsbaseurl/v3.6/leaf/{{lowerId}}/{{version}}/index.json",
                                                                   "catalogEntry": {
                                                                       "@id": "http://localhost/registrationsbaseurl/v3.6/catalog/{{lowerId}}/{{version}}/index.json",
                                                                       "id": "{{id}}",
                                                                       "version": "{{version}}"
                                                                   },
                                                                   "packageContent": "http://localhost/packagebaseaddress/v3/{{lowerId}}/{{version}}/{{lowerId}}.{{version}}.nupkg"
                                                               }
                                                           ],
                                                           "lower": "{{version}}",
                                                           "upper": "{{version}}",
                                                           "parent": "http://localhost/registrationsbaseurl/v3.6/{lowerId}/index.json"
                                                       }
                                                   ]
                                               }
                                               """;

        this._mockHttp.Expect($"http://localhost/registrationsbaseurl/v3.6/{lowerId}/index.json").Respond("application/json", registrationIndexJson);

        var reg = await this._annaClient.GetRegistrationIndex(id);
        reg.ShouldNotBeNull();

        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestSearch()
    {
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/search/v3.5\", \"@type\": \"SearchQueryService/3.5.0\"}]}");

        this._mockHttp.Expect("http://localhost/search/v3.5")
            .Respond("application/json", "{\"totalHits\": 0, \"results\": []}");

        var response = await this._annaClient.Search(new SearchRequest());

        response.ShouldNotBeNull();
        response.TotalHits.ShouldBe(0);
        response.Results.ShouldBeEmpty();

        this._mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestSearchWithFields()
    {
        this._mockHttp.Expect(this._annaClientOptions.IndexUrl.ToString())
            .Respond("application/json",
                     "{\"version\": \"3.0.0\", \"resources\": [{\"@id\": \"http://localhost/search/v3.5\", \"@type\": \"SearchQueryService/3.5.0\"}]}");

        this._mockHttp.Expect("http://localhost/search/v3.5")
            .WithQueryString([
                new KeyValuePair<string, string>("q", "Microsoft"),
                new KeyValuePair<string, string>("prerelease", "True")
            ])
            .Respond("application/json", "{\"totalHits\": 0, \"results\": []}");

        var response = await this._annaClient.Search(new SearchRequest { Query = "Microsoft", Prerelease = true });

        response.ShouldNotBeNull();
        response.TotalHits.ShouldBe(0);
        response.Results.ShouldBeEmpty();

        this._mockHttp.VerifyNoOutstandingExpectation();
    }
}
