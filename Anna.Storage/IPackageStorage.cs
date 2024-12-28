using NuGet.Versioning;

namespace Anna.Storage;

public interface IPackageStorage
{
    Stream GetPackage(string name, NuGetVersion version);
    Stream GetPackageManifest(string name, NuGetVersion version);
    Task PutPackage(string name, NuGetVersion version, Stream data);
    Task PutPackageManifest(string name, NuGetVersion version, Stream data);
    void DeletePackage(string name, NuGetVersion version);
}
