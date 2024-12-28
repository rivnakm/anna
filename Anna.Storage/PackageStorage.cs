using System.IO.Abstractions;
using NuGet.Versioning;

namespace Anna.Storage;

public class PackageStorage : IPackageStorage
{
    private readonly IFileSystem _filesystem;
    private readonly string _storageRootDir;

    public PackageStorage(string storageRootDir) : this(storageRootDir, new FileSystem()) { }

    public PackageStorage(string storageRootDir, IFileSystem filesystem)
    {
        this._storageRootDir = storageRootDir;
        this._filesystem = filesystem;
    }

    public Stream GetPackage(string name, NuGetVersion version)
    {
        return this.GetFile(Path.Combine(this.PackageDirectory(name, version), PackageFileName(name, version)));
    }

    public Stream GetPackageManifest(string name, NuGetVersion version)
    {
        return this.GetFile(Path.Combine(this.PackageDirectory(name, version), PackageManifestFileName(name, version)));
    }

    public async Task PutPackage(string name, NuGetVersion version, Stream data)
    {
        await this.CreateFile(Path.Combine(this.PackageDirectory(name, version), PackageFileName(name, version)), data);
    }

    public async Task PutPackageManifest(string name, NuGetVersion version, Stream data)
    {
        await this.CreateFile(Path.Combine(this.PackageDirectory(name, version), PackageManifestFileName(name, version)), data);
    }

    public Task DeletePackage(string name, NuGetVersion version)
    {
        throw new NotImplementedException();
    }

    private string PackageDirectory(string name, NuGetVersion version)
    {
        return Path.Combine([
            this._storageRootDir,
            char.ToLowerInvariant(name.First()).ToString(),
            name,
            version.ToString()
        ]);
    }

    private static string PackageFileName(string name, NuGetVersion version)
    {
        return $"{name}.{version.ToString()}.nupkg";
    }

    private static string PackageManifestFileName(string name, NuGetVersion version)
    {
        return $"{name}.{version.ToString()}.nuspec";
    }

    private Stream GetFile(string path)
    {
        return this._filesystem.File.OpenRead(path);
    }

    private async Task CreateFile(string path, Stream data)
    {
        var parentDir = Path.GetDirectoryName(path);
        if (parentDir is not null && !this._filesystem.Directory.Exists(parentDir))
        {
            this._filesystem.Directory.CreateDirectory(parentDir);
        }
        using var fileStream = this._filesystem.File.OpenWrite(path);

        await data.CopyToAsync(fileStream);
    }
}
