using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
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
}
