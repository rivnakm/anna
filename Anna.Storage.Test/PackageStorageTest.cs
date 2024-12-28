using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NuGet.Versioning;

namespace Anna.Storage.Test;

public class PackageStorageTest
{
    [Fact]
    public void TestGetPackage()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData> {
            {$"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nupkg", new MockFileData(fileContents)}
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        var package = packageStorage.GetPackage("Foo", new NuGetVersion(1, 0, 0));

        using var streamReader = new StreamReader(package);
        streamReader.ReadToEnd().Should().Be(fileContents);
    }

    [Fact]
    public void TestGetPackageManifest()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData> {
            {$"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nuspec", new MockFileData(fileContents)}
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        var package = packageStorage.GetPackageManifest("Foo", new NuGetVersion(1, 0, 0));

        using var streamReader = new StreamReader(package);
        streamReader.ReadToEnd().Should().Be(fileContents);
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
        filesystem.File.Exists(filePath).Should().BeTrue();

        var actualContents = filesystem.File.ReadAllText(filePath);
        actualContents.Should().Be(fileContents);
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
        filesystem.File.Exists(filePath).Should().BeTrue();

        var actualContents = filesystem.File.ReadAllText(filePath);
        actualContents.Should().Be(fileContents);
    }

    [Fact]
    public void TestDeletePackage()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData> {
            {$"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nupkg", new MockFileData(fileContents)},
            {$"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nuspec", new MockFileData(fileContents)},
            {$"{storageRootDir}/f/Foo/2.0.0/Foo.2.0.0.nupkg", new MockFileData(fileContents)},
            {$"{storageRootDir}/f/Foo/2.0.0/Foo.2.0.0.nuspec", new MockFileData(fileContents)}
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        packageStorage.DeletePackage("Foo", new NuGetVersion(1, 0, 0));

        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nupkg")).Should().BeFalse();
        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nuspec")).Should().BeFalse();

        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/2.0.0/Foo.2.0.0.nupkg")).Should().BeTrue();
        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/2.0.0/Foo.2.0.0.nuspec")).Should().BeTrue();
    }

    [Fact]
    public void TestDeletePackage_LastVersion_DeletesPackageDir()
    {
        const string storageRootDir = "/app/anna/packages";
        const string fileContents = "foobar";
        var filesystem = new MockFileSystem(new Dictionary<string, MockFileData> {
            {$"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nupkg", new MockFileData(fileContents)},
            {$"{storageRootDir}/f/Foo/1.0.0/Foo.1.0.0.nuspec", new MockFileData(fileContents)},
        });

        var packageStorage = new PackageStorage(storageRootDir, filesystem);

        packageStorage.DeletePackage("Foo", new NuGetVersion(1, 0, 0));

        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nupkg")).Should().BeFalse();
        filesystem.File.Exists(Path.Combine(storageRootDir, "f/Foo/1.0.0/Foo.1.0.0.nuspec")).Should().BeFalse();
        filesystem.Directory.Exists(Path.Combine(storageRootDir, "f/Foo")).Should().BeFalse();
    }
}
