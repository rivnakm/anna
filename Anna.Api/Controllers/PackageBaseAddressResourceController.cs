using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Api.Models;
using Anna.Index;
using Anna.Index.Exceptions;
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
    public async Task<IActionResult> GetPackageVersions(string lowerId)
    {
        try
        {
            var versions = (await this._packageIndex.GetVersions(lowerId)).Select(v => v.ToString()).ToList();

            var response = new GetPackageVersionsResponse
            {
                Versions = versions
            };

            return new OkObjectResult(response);
        }
        catch (PackageNotFoundException)
        {
            return new NotFoundResult();
        }
    }

    [Route("{lowerId}/{lowerVersion}/{lowerFileName}.nupkg")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPackageFile(string lowerId, string lowerVersion, string lowerFileName)
    {
        if (lowerFileName != $"{lowerId}.{lowerVersion}")
        {
            return new NotFoundResult();
        }

        try
        {
            var name = await this._packageIndex.GetPackageName(lowerId);
            var version = (await this._packageIndex.GetVersions(lowerId))
                    .SingleOrDefault(v => v.ToString().ToLowerInvariant() == lowerVersion);
            if (version is null)
            {
                return new NotFoundResult();
            }

            var fileStreamResult = new FileStreamResult(this._packageStorage.GetPackage(name, version), MimeTypes.Application.OctetStream);

            fileStreamResult.FileDownloadName = $"{lowerId}.{lowerVersion}.nupkg";

            return fileStreamResult;
        }
        catch (PackageNotFoundException)
        {
            return new NotFoundResult();
        }
    }

    [Route("{lowerId}/{lowerVersion}/{lowerFileName}.nuspec")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPackageManifest(string lowerId, string lowerVersion, string lowerFileName)
    {
        if (lowerFileName != $"{lowerId}.{lowerVersion}")
        {
            return new NotFoundResult();
        }

        try
        {
            var name = await this._packageIndex.GetPackageName(lowerId);
            var version = (await this._packageIndex.GetVersions(lowerId))
                    .SingleOrDefault(v => v.ToString().ToLowerInvariant() == lowerVersion);
            if (version is null)
            {
                return new NotFoundResult();
            }

            var fileStreamResult = new FileStreamResult(this._packageStorage.GetPackageManifest(name, version), MimeTypes.Application.Xml);

            fileStreamResult.FileDownloadName = $"{lowerId}.{lowerVersion}.nuspec";

            return fileStreamResult;
        }
        catch (PackageNotFoundException)
        {
            return new NotFoundResult();
        }
    }
}
