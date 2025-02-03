using System.Collections.Generic;
using System.Threading.Tasks;
using Anna.Index.Models;
using NuGet.Versioning;

namespace Anna.Index;

public interface IPackageIndex
{
    // TODO: document
    Task AddPackage(string name, NuGetVersion version);
    Task UnlistPackage(string name, NuGetVersion version);
    Task RelistPackage(string name, NuGetVersion version);
    Task RemovePackage(string name, NuGetVersion version);

    IAsyncEnumerable<NuGetVersion> GetVersions(string lowerName);
    Task<string> GetPackageName(string lowerName);
    Task<CatalogEntry> GetCatalog(string lowerName, NuGetVersion version);
}
