using System.Threading.Tasks;
using Anna.Api.Controllers;
using Anna.Api.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Anna.Api.Test.Controllers;

public class IndexControllerTest
{
    [Fact]
    public async Task TestGetIndex_NoResources()
    {
        var apiDescriptionGroupCollectionProvider = A.Fake<IApiDescriptionGroupCollectionProvider>();
        var controller = new IndexController(apiDescriptionGroupCollectionProvider);

        var resp = await controller.GetIndex();

        resp.Should().BeOfType<OkObjectResult>();

        var index = ((OkObjectResult)resp).Value as GetIndexResponse;

        index.Should().NotBeNull();

        index!.Version.Should().Be("3.0.0");
        index!.Resources.Should().BeEmpty();
    }
}
