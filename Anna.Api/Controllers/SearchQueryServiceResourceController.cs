using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Common.Models.SearchQueryService;
using Anna.Index;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anna.Api.Controllers;

[Route("/searchqueryservice/v3.5")]
[Resource("/searchqueryservice/v3.5", "SearchQueryService", "3.5.0")]
[ApiController]
public class SearchQueryServiceResourceController : ResourceController
{
    private readonly IPackageIndex _packageIndex;

    public SearchQueryServiceResourceController(IPackageIndex packageIndex)
    {
        this._packageIndex = packageIndex;
    }

    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> Search([FromQuery] string q = "", [FromQuery] int skip = 0,
        [FromQuery]
        int take = 100, [FromQuery] bool prerelease = false, [FromQuery] string? semVerLevel = null,
        [FromQuery]
        string? packageType = null)
    {
        if (semVerLevel != null || packageType != null)
        {
            return new StatusCodeResult(StatusCodes.Status501NotImplemented);
        }

        var results = (await this._packageIndex.QueryPackages(q, skip, take, prerelease)).Select(p => new SearchResult
        {
            Id = p.Name,
            Version = p.Versions.First().PackageVersion.ToString(),
            Versions = p.Versions.Where(v => !v.PackageVersion.IsPrerelease || prerelease).Select(
            v => new SearchResultVersion
            {
                Id = p.Name,
                Version = v.PackageVersion.ToString(),
                Downloads = 0
            }).ToList(),
            PackageTypes = []
        }).ToList();
        var resp = new SearchResponse
        {
            TotalHits = await this._packageIndex.CountPackages(q, prerelease),
            Results = results
        };

        return new OkObjectResult(resp);
    }
}
