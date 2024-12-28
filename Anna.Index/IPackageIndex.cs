using NuGet.Versioning;

namespace Anna.Index;

public interface IPackageIndex
{
    // TODO: document
    Task<IEnumerable<NuGetVersion>> GetVersions(string lowerName);
    Task<string?> GetPackageName(string lowerName);
    Task AddPackage(string name, NuGetVersion version);
    Task UnlistPackage(string name, NuGetVersion version);
    Task RelistPackage(string name, NuGetVersion version);
    Task RemovePackage(string name, NuGetVersion version);
}
