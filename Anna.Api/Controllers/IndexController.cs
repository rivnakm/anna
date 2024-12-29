using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Anna.Api.Controllers;

[Route("/v3/index.json")]
[ApiController]
public class IndexController : ControllerBase
{
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;

    public IndexController(IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider)
    {
        this._apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
    }

    [HttpGet]
    [HttpHead]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetIndexResponse))]
    public Task<IActionResult> GetIndex()
    {
        return Task.FromResult<IActionResult>(new OkObjectResult(new GetIndexResponse
        {
            Version = "3.0.0",
            Resources = this._apiDescriptionGroupCollectionProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(desc => desc.RelativePath is not null)
                .Where(desc => desc.ActionDescriptor.Properties.ContainsKey(typeof(ResourceAttribute)))
                .DistinctBy(desc => desc.ActionDescriptor.GetProperty<ResourceAttribute>()!.ResourceType)
                .Select(desc =>
                {
                    var attr = desc.ActionDescriptor.GetProperty<ResourceAttribute>()!;
                    return new GetIndexResponse.Resource
                    {
                        Id = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{attr.Path}",
                        Type = attr.ResourceType,
                    };
                }).ToList()
        }));
    }
}
