using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Api.Models.RegistrationIndex;
using Anna.Api.Resources;
using Anna.Index;
using Anna.Index.Exceptions;
using Anna.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Versioning;

namespace Anna.Api.Controllers;

[Route("/registrationbaseurl/v3.6")]
[Resource("/registrationbaseurl/v3.6/", "RegistrationsBaseUrl", "3.6.0")]
[ApiController]
public class RegistrationsBaseUrlResourceController : ResourceController
{
    private readonly IPackageIndex _packageIndex;
    private readonly IPackageStorage _packageStorage;
    private readonly IResourceProvider _resourceProvider;

    private const int PAGE_SIZE = 64;

    public RegistrationsBaseUrlResourceController(IPackageIndex packageIndex, IPackageStorage packageStorage, IResourceProvider resourceProvider)
    {
        this._packageIndex = packageIndex;
        this._packageStorage = packageStorage;
        this._resourceProvider = resourceProvider;
    }

    [Route("{lowerId}/index.json")]
    [HttpGet]
    [HttpHead]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegistrationIndexDto))]
    public async Task<IActionResult> GetRegistrationIndex(string lowerId)
    {
        try
        {
            var versions = await this._packageIndex.GetVersions(lowerId).ToListAsync();

            versions.Sort();
            var inlinePages = versions.Count < (2 * PAGE_SIZE);

            var baseId = this._resourceProvider.GetResources().Single(r => r.TypeName == "RegistrationsBaseUrl").Id;


            var index = new RegistrationIndexDto
            {
                Count = 0,
                Items = []
            };
            // TODO: non-inlined pages
            foreach (var chunk in versions.Chunk(PAGE_SIZE))
            {
                if (chunk.Length == 0)
                {
                    continue;
                }

                var lower = chunk.Min();
                var upper = chunk.Max();
                var id = Path.Combine([baseId, "page", lowerId, lower!.ToString(), "index.json"]);
                var items = await Task.WhenAll(chunk.Select(v => this.GetRegistrationLeaf(lowerId, v)));

                var page = new RegistrationPageDto
                {
                    Id = id,
                    Lower = lower!.ToString(),
                    Upper = upper!.ToString(),
                    Count = chunk.Length,
                    Parent = Path.Combine(baseId, lowerId, "index.json"),
                    Items = items.ToList(),
                };

                index.Items.Add(page);
            }

            index.Count = index.Items.Count;

            return new OkObjectResult(index);
        }
        catch (PackageNotFoundException)
        {
            return new NotFoundResult();
        }
    }

    [Route("page/{lowerId}/{minVersion}/index.json")]
    [HttpGet]
    [HttpHead]
    public Task<IActionResult> GetRegistrationPage(string lowerId, string minVersion)
    {
        return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status501NotImplemented));
    }

    [Route("leaf/{lowerId}/{version}/index.json")]
    [HttpGet]
    [HttpHead]
    public Task<IActionResult> GetRegistrationLeaf(string lowerId, string version)
    {
        return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status501NotImplemented));
    }

    [Route("catalog/{lowerId}/{version}/index.json")]
    [HttpGet]
    [HttpHead]
    public Task<IActionResult> GetCatalogEntry(string lowerId, string version)
    {
        return Task.FromResult<IActionResult>(new StatusCodeResult(StatusCodes.Status501NotImplemented));
    }

    private async Task<RegistrationLeafDto> GetRegistrationLeaf(string lowerName, NuGetVersion version)
    {
        var baseId = this._resourceProvider.GetResources().Single(r => r.TypeName == "RegistrationsBaseUrl").Id;
        var contentUrlBase = this._resourceProvider.GetResources().Single(r => r.TypeName == "PackageBaseAddress").Id;
        // TODO: does the version need to be normalized here? (3.6.0 returns semver 2 so maybe not)
        var contentUrl = Path.Combine([contentUrlBase, lowerName, version.ToString().ToLowerInvariant(), $"{lowerName}.{version.ToString().ToLowerInvariant()}.nupkg"]);

        var entry = await this._packageIndex.GetCatalog(lowerName, version);
        return new RegistrationLeafDto
        {
            Id = Path.Combine([baseId, "leaf", lowerName, version.ToString()]),
            PackageContent = contentUrl,
            CatalogEntry = new CatalogEntryDto {
                Id = Path.Combine([baseId, "catalog", lowerName, version.ToString()]),
                PackageId = entry.PackageId,
                Version = entry.Version.ToString()
            }
        };
    }
}
