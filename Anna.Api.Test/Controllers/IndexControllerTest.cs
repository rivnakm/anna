using System.Collections.Generic;
using System.Threading.Tasks;
using Anna.Api.Controllers;
using Anna.Api.Models;
using Anna.Api.Resources;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
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

    [Fact]
    public async Task TestGetIndex_ForwardedProto()
    {
        var resourceProvider = A.Fake<IResourceProvider>();
        var controller = new IndexController(resourceProvider);

        A.CallTo(() => resourceProvider.GetResources()).Returns(new List<Resource> {
            new Resource {
                Id = "/packagebaseaddress/v3/",
                TypeName = "PackageBaseAddress",
                TypeVersion = "3.0.0"
            }
        });

        var request = A.Fake<HttpRequest>();
        A.CallTo(() => request.Headers).Returns(new HeaderDictionary()
        {
            ["x-forwarded-proto"] = "https"
        });
        A.CallTo(() => request.Scheme).Returns("http");
        A.CallTo(() => request.Host).Returns(new HostString("nuget.org"));

        var context = A.Fake<HttpContext>();
        A.CallTo(() => context.Request).Returns(request);

        controller.ControllerContext.HttpContext = context;

        var resp = await controller.GetIndex();

        resp.ShouldBeOfType<OkObjectResult>();

        var index = ((OkObjectResult)resp).Value as IndexDto;

        index.ShouldNotBeNull();

        index!.Version.ShouldBe("3.0.0");
        index!.Resources.Count.ShouldBe(1);
        index!.Resources[0].Id.ShouldStartWith("https");
    }
}
