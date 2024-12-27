using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Api.Models;
using Anna.Index;
using Anna.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anna.Api.Controllers;

[Route("/v3-flatcontainer")]
[Resource("/v3-flatcontainer/", "PackageBaseAddress", "3.0.0")]
[ApiController]
public class PackageBaseAddressResourceController : ResourceController
{
    private readonly IPackageIndex _packageIndex;
    private readonly IPackageStorage _packageStorage;

    public PackageBaseAddressResourceController(IPackageIndex packageIndex, IPackageStorage packageStorage)
    {
        this._packageIndex = packageIndex;
        this._packageStorage = packageStorage;
    }

    [Route("{lowerId}/index.json")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetPackageVersions(string lowerId)
    {
        var versions = this._packageIndex.GetVersions(lowerId).Select(v => v.ToString()).ToList();

        if (versions.Count == 0)
        {
            return Task.FromResult<IActionResult>(new NotFoundResult());
        }

        var response = new GetPackageVersionsResponse
        {
            Versions = versions
        };

        return Task.FromResult<IActionResult>(new OkObjectResult(response));
    }

    [Route("{lowerId}/{lowerVersion}/{lowerFileName}.nupkg")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetPackageFile(string lowerId, string lowerVersion, string lowerFileName)
    {
        if (lowerFileName != $"{lowerId}.{lowerVersion}")
        {
            return Task.FromResult<IActionResult>(new NotFoundResult());
        }

        var name = this._packageIndex.GetPackageName(lowerId);
        if (name is null)
        {
            return Task.FromResult<IActionResult>(new NotFoundResult());
        }

        var version = this._packageIndex.GetVersions(lowerId)
                .SingleOrDefault(v => v.ToString().ToLowerInvariant() == lowerVersion);
        if (version is null)
        {
            return Task.FromResult<IActionResult>(new NotFoundResult());
        }

        var fileStreamResult = new FileStreamResult(this._packageStorage.GetPackage(name, version), MimeTypes.Application.OctetStream);

        fileStreamResult.FileDownloadName = $"{lowerId}.{lowerVersion}.nupkg";

        return Task.FromResult<IActionResult>(fileStreamResult);
    }
}
