using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using NuGet.Versioning;
using Shouldly;

namespace Anna.Storage.Test;

public class PackageStorageTest
{
    [Fact]
    public void TestGetPackage()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nupkg", new MockFileData(fileContents) }
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        var package = packageStorage.GetPackage("Foo", new NuGetVersion(1, 0, 0));

        using var streamReader = new StreamReader(package);
        streamReader.ReadToEnd().ShouldBe(fileContents);
    }

    [Fact]
    public void TestGetPackageManifest()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nuspec", new MockFileData(fileContents) }
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        var package = packageStorage.GetPackageManifest("Foo", new NuGetVersion(1, 0, 0));

        using var streamReader = new StreamReader(package);
        streamReader.ReadToEnd().ShouldBe(fileContents);
    }

    [Fact]
    public async Task TestPutPackage()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";

        var filesystem = new MockFileSystem();
        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));

        await packageStorage.PutPackage("Foo", new NuGetVersion(1, 0, 0), stream);

        var filePath = Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nupkg");
        filesystem.File.Exists(filePath).ShouldBeTrue();

        var actualContents = filesystem.File.ReadAllText(filePath);
        actualContents.ShouldBe(fileContents);
    }

    [Fact]
    public async Task TestPutPackageManifest()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";

        var filesystem = new MockFileSystem();
        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));

        await packageStorage.PutPackageManifest("Foo", new NuGetVersion(1, 0, 0), stream);

        var filePath = Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nuspec");
        filesystem.File.Exists(filePath).ShouldBeTrue();

        var actualContents = filesystem.File.ReadAllText(filePath);
        actualContents.ShouldBe(fileContents);
    }

    [Fact]
    public void TestDeletePackage()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nupkg", new MockFileData(fileContents) },
            { $"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nuspec", new MockFileData(fileContents) },
            { $"{storageRootDir}/f/Foo/2.0.0/Foo.2.0.0.nupkg", new MockFileData(fileContents) },
            { $"{storageRootDir}/f/Foo/2.0.0/Foo.2.0.0.nuspec", new MockFileData(fileContents) }
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        packageStorage.DeletePackage("Foo", new NuGetVersion(1, 0, 0));

        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nupkg")).ShouldBeFalse();
        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nuspec")).ShouldBeFalse();

        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/2.0.0/Foo.2.0.0.nupkg")).ShouldBeTrue();
        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/2.0.0/Foo.2.0.0.nuspec")).ShouldBeTrue();
    }

    [Fact]
    public void TestDeletePackage_LastVersion_DeletesPackageDir()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nupkg", new MockFileData(fileContents) },
            { $"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nuspec", new MockFileData(fileContents) }
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        packageStorage.DeletePackage("Foo", new NuGetVersion(1, 0, 0));

        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nupkg")).ShouldBeFalse();
        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nuspec")).ShouldBeFalse();
        filesystem.Directory.Exists(Path.Combine(storageRootDir, "f/Foo")).ShouldBeFalse();
    }
}
