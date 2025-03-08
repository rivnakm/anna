using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anna.Api.Controllers;
using Anna.Common.Models;
using Anna.Index;
using Anna.Index.Exceptions;
using Anna.Storage;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NuGet.Versioning;
using Shouldly;

namespace Anna.Api.Test.Controllers;

public class PackageBaseAddressResourceControllerTest
{
    [Fact]
    public async Task TestGetPackageVersions()
    {
        const string packageLowerName = "foo";
        var expectedVersions = new List<NuGetVersion>
        {
            new(1, 0, 0),
            new(2, 0, 0)
        };

        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(Task.FromResult<IEnumerable<NuGetVersion>>(expectedVersions));

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageVersions(packageLowerName);

        resp.ShouldBeOfType<OkObjectResult>();

        var value = ((OkObjectResult)resp).Value;
        value.ShouldNotBeNull();
        value.ShouldBeOfType<PackageVersions>();

        var pkgVersionsResp = (PackageVersions)value;

        pkgVersionsResp.Versions.ShouldBe(expectedVersions.Select(v => v.ToString()).ToList());
    }

    [Fact]
    public async Task TestGetPackageVersions_NoMatch_Returns404()
    {
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).Throws(new PackageNotFoundException());

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageVersions("foo");

        resp.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task TestGetPackageFile()
    {
        const string packageName = "Foo";
        const string packageLowerName = "foo";
        const string packageLowerVersion = "2.0.0";
        var availableVersions = new List<NuGetVersion>
        {
            new(1, 0, 0),
            new(2, 0, 0)
        };

        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(Task.FromResult<IEnumerable<NuGetVersion>>(availableVersions));
        A.CallTo(() => packageIndex.GetPackageName(packageLowerName)).Returns(packageName);
        A.CallTo(() => packageStorage.GetPackage(packageName,
                                                 A<NuGetVersion>.That.Matches(
                                                 v => v.ToString().ToLowerInvariant() == packageLowerVersion)))
            .Returns(new MemoryStream());

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName,
                                                   packageLowerVersion,
                                                   $"{packageLowerName}.{packageLowerVersion}");

        resp.ShouldBeOfType<FileStreamResult>();

        var fileStreamResult = (FileStreamResult)resp;
        fileStreamResult.ContentType.ShouldBe(MimeTypes.Application.OctetStream);
        fileStreamResult.FileDownloadName.ShouldBe($"{packageLowerName}.{packageLowerVersion}.nupkg");
    }

    [Fact]
    public async Task TestGetPackageFile_NonMatchingIdVersionAndFilename_Returns404()
    {
        const string packageLowerName = "foo";
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName, "1.0.0", $"{packageLowerName}.2.0.0");

        resp.ShouldBeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestGetPackageFile_NoIdMatch_Returns404()
    {
        const string packageLowerName = "foo";
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetPackageName(packageLowerName)).Throws(new PackageNotFoundException());

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName, "1.0.0", $"{packageLowerName}.1.0.0");

        resp.ShouldBeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestGetPackageFile_NoVersionMatch_Returns404()
    {
        const string packageLowerName = "foo";
        var expectedVersions = new List<NuGetVersion>
        {
            new(1, 0, 0),
            new(2, 0, 0)
        };
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(Task.FromResult<IEnumerable<NuGetVersion>>(expectedVersions));

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageFile(packageLowerName, "3.0.0", $"{packageLowerName}.3.0.0");

        resp.ShouldBeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async Task TestGetPackageManifest()
    {
        const string packageName = "Foo";
        const string packageLowerName = "foo";
        const string packageLowerVersion = "2.0.0";
        var availableVersions = new List<NuGetVersion>
        {
            new(1, 0, 0),
            new(2, 0, 0)
        };

        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(Task.FromResult<IEnumerable<NuGetVersion>>(availableVersions));
        A.CallTo(() => packageIndex.GetPackageName(packageLowerName)).Returns(packageName);
        A.CallTo(() => packageStorage.GetPackageManifest(packageName,
                                                         A<NuGetVersion>.That.Matches(
                                                         v => v.ToString().ToLowerInvariant() == packageLowerVersion)))
            .Returns(new MemoryStream());

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageManifest(packageLowerName,
                                                       packageLowerVersion,
                                                       $"{packageLowerName}.{packageLowerVersion}");

        resp.ShouldBeOfType<FileStreamResult>();

        var fileStreamResult = (FileStreamResult)resp;
        fileStreamResult.ContentType.ShouldBe(MimeTypes.Application.Xml);
        fileStreamResult.FileDownloadName.ShouldBe($"{packageLowerName}.{packageLowerVersion}.nuspec");
    }

    [Fact]
    public async Task TestGetPackageManifest_NonMatchingIdVersionAndFilename_Returns404()
    {
        const string packageLowerName = "foo";
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageManifest(packageLowerName, "1.0.0", $"{packageLowerName}.2.0.0");

        resp.ShouldBeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestGetPackageManifest_NoIdMatch_Returns404()
    {
        const string packageLowerName = "foo";
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetPackageName(packageLowerName)).Throws(new PackageNotFoundException());

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageManifest(packageLowerName, "1.0.0", $"{packageLowerName}.1.0.0");

        resp.ShouldBeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestGetPackageManifest_NoVersionMatch_Returns404()
    {
        const string packageLowerName = "foo";
        var expectedVersions = new List<NuGetVersion>
        {
            new(1, 0, 0),
            new(2, 0, 0)
        };
        var packageIndex = A.Fake<IPackageIndex>();
        var packageStorage = A.Fake<IPackageStorage>();
        A.CallTo(() => packageIndex.GetVersions(packageLowerName)).Returns(Task.FromResult<IEnumerable<NuGetVersion>>(expectedVersions));

        var controller = new PackageBaseAddressResourceController(packageIndex, packageStorage);
        var resp = await controller.GetPackageManifest(packageLowerName, "3.0.0", $"{packageLowerName}.3.0.0");

        resp.ShouldBeOfType<NotFoundResult>();

        A.CallTo(() => packageIndex.GetPackageName(A<string>._)).MustHaveHappened();
        A.CallTo(() => packageIndex.GetVersions(A<string>._)).MustHaveHappened();
    }
}
