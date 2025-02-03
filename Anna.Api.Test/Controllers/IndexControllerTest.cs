using System.Threading.Tasks;
using Anna.Api.Controllers;
using Anna.Api.Models;
using Anna.Api.Resources;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Anna.Api.Test.Controllers;

public class IndexControllerTest
{
    [Fact]
    public async Task TestGetIndex_NoResources()
    {
        var resourceProvider = A.Fake<IResourceProvider>();
        var controller = new IndexController(resourceProvider);

        var resp = await controller.GetIndex();

        resp.ShouldBeOfType<OkObjectResult>();

        var index = ((OkObjectResult)resp).Value as IndexDto;

        index.ShouldNotBeNull();

        index!.Version.ShouldBe("3.0.0");
        index!.Resources.ShouldBeEmpty();
    }
}
