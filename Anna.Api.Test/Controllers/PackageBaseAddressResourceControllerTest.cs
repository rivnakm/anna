using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Controllers;
using Anna.Api.Models;
using Anna.Index;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Semver;

namespace Anna.Api.Test.Controllers;

public class PackageBaseAddressResourceControllerTest
{
    [Fact]
    public async Task TestGetPackageVersions()
    {
        const string packageLowerName = "foo";
        var expectedVersions = new List<SemVersion> {
            new SemVersion(1, 0, 0),
            new SemVersion(2, 0, 0)
        };

        var packageIndex = A.Fake<IPackageIndex>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(expectedVersions);

        var controller = new PackageBaseAddressResourceController(packageIndex);
        var resp = await controller.GetPackageVersions(packageLowerName);

        resp.Should().BeOfType<OkObjectResult>();

        var value = ((OkObjectResult)resp).Value;
        value.Should().NotBeNull();
        value.Should().BeOfType<GetPackageVersionsResponse>();

        var pkgVersionsResp = (GetPackageVersionsResponse)value!;

        pkgVersionsResp.Versions.Should().Equal(expectedVersions.Select(v => v.ToString()).ToList());
    }

    [Fact]
    public async Task TestGetPackageVersions_NoMatch_Returns404()
    {
        var packageIndex = A.Fake<IPackageIndex>();
        var controller = new PackageBaseAddressResourceController(packageIndex);
        var resp = await controller.GetPackageVersions("foo");

        resp.Should().BeOfType<NotFoundResult>();
    }
}
