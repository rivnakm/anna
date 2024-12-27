using System.IO.Abstractions;
using Semver;

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

    public Stream GetPackage(string name, SemVersion version)
    {
        return this._filesystem.File.OpenRead(this.PackageFilePath(name, version));
    }

    public void PutPackage(string name, SemVersion version, Stream data)
    {
        throw new NotImplementedException("PackageStorage.PutPackage() is not implemented");
    }

    private string PackageFilePath(string name, SemVersion version)
    {
        return Path.Combine([
            this._storageRootDir,
            char.ToLowerInvariant(name.First()).ToString(),
            name,
            version.ToString(),
            PackageFileName(name, version)
        ]);
    }

    private static string PackageFileName(string name, SemVersion version)
    {
        return $"{name}.{version.ToString()}.nupkg";
    }
}
