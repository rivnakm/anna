using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Api.Models;
using Anna.Index;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anna.Api.Controllers;

[Route("/v3-flatcontainer")]
[Resource("/v3-flatcontainer/", "PackageBaseAddress", "3.0.0")]
[ApiController]
public class PackageBaseAddressResourceController : ResourceController
{
    private readonly IPackageIndex _packageIndex;
    public PackageBaseAddressResourceController(IPackageIndex packageIndex)
    {
        this._packageIndex = packageIndex;
    }

    [Route("/{lowerId}/index.json")]
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
}
