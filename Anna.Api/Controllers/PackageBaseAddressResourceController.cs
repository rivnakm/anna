using System;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Index;
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
    public Task<IActionResult> GetPackageVersions(string lowerId)
    {
        throw new NotImplementedException();
    }
}
