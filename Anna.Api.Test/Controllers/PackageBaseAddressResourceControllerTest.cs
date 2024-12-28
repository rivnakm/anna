using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Controllers;
using Anna.Api.Models;
using Anna.Index;
using Anna.Storage;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NuGet.Versioning;

namespace Anna.Api.Test.Controllers;

public class PackageBaseAddressResourceControllerTest
{
    [Fact]
    public async Task TestGetPackageVersions()
    {
        const string packageLowerName = "foo";
        var expectedVersions = new List<NuGetVersion> {
            new NuGetVersion(1, 0, 0),
            new NuGetVersion(2, 0, 0)
        };

        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(expectedVersions);

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
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
        var packageStorage = A.Fake<IPackageStorage>();
        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageVersions("foo");

        resp.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task TestGetPackageFile()
    {
        const string packageName = "Foo";
        const string packageLowerName = "foo";
        const string packageLowerVersion = "2.0.0";
        var availableVersions = new List<NuGetVersion> {
            new NuGetVersion(1, 0, 0),
            new NuGetVersion(2, 0, 0)
        };

        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(availableVersions);
        A.CallTo(() => packageIndex.GetPackageName(packageLowerName)).Returns(packageName);
        A.CallTo(() => packageStorage.GetPackage(packageName, A<NuGetVersion>.That.Matches(v => v.ToString().ToLowerInvariant() == packageLowerVersion))).Returns(new MemoryStream());

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName, packageLowerVersion, $"{packageLowerName}.{packageLowerVersion}");

        resp.Should().BeOfType<FileStreamResult>();

        var fileStreamResult = (FileStreamResult)resp;
        fileStreamResult.ContentType.Should().Be(MimeTypes.Application.OctetStream);
        fileStreamResult.FileDownloadName.Should().Be($"{packageLowerName}.{packageLowerVersion}.nupkg");
    }

    [Fact]
    public async Task TestGetPackageFile_NonMatchingIdVersionAndFilename_Returns404()
    {
        const string packageLowerName = "foo";
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetPackageName(packageLowerName)).Returns(Task.FromResult<string?>(null));

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName, "1.0.0", $"{packageLowerName}.2.0.0");

        resp.Should().BeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestGetPackageFile_NoIdMatch_Returns404()
    {
        const string packageLowerName = "foo";
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetPackageName(packageLowerName)).Returns(Task.FromResult<string?>(null));

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName, "1.0.0", $"{packageLowerName}.1.0.0");

        resp.Should().BeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestGetPackageFile_NoVersionMatch_Returns404()
    {
        const string packageLowerName = "foo";
        var expectedVersions = new List<NuGetVersion> {
            new NuGetVersion(1, 0, 0),
            new NuGetVersion(2, 0, 0)
        };
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(expectedVersions);

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName, "3.0.0", $"{packageLowerName}.3.0.0");

        resp.Should().BeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustHaveHappened();
    }
}
