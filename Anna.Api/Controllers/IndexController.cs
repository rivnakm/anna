using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Anna.Api.Models;
using Anna.Api.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Anna.Api.Controllers;

[Route("/v3/index.json")]
[ApiController]
public class IndexController : ControllerBase
{
    private readonly IResourceProvider _resourceProvider;

    public IndexController(IResourceProvider resourceProvider)
    {
        this._resourceProvider = resourceProvider;
    }

    [HttpGet]
    [HttpHead]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IndexDto))]
    public Task<IActionResult> GetIndex()
    {
        return Task.FromResult<IActionResult>(new OkObjectResult(new IndexDto
        {
            Version = "3.0.0",
            Resources = this._resourceProvider.GetResources()
                .Select(res =>
                {
                    Console.WriteLine(JsonSerializer.Serialize(HttpContext.Request.Headers.ToDictionary()));
                    var scheme = HttpContext.Request.Scheme;
                    if (HttpContext.Request.Headers.TryGetValue("x-forwarded-proto", out var schemeValues))
                    {
                        scheme = schemeValues.First();
                    }
                    return new IndexDto.Resource
                    {
                        Id = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{res.Id}",
                        Type = $"{res.TypeName}/{res.TypeVersion}",
                    };
                }).ToList()
        }));
    }
}
