using Anna.Index.Db;
using Semver;

namespace Anna.Index;

public class PackageIndex : IPackageIndex
{
    private readonly IndexContext _dbContext;

    public PackageIndex(IndexContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public IEnumerable<SemVersion> GetVersions(string lowerName)
    {
        var package = this._dbContext.Packages.SingleOrDefault(p => p.LowerName == lowerName);
        if (package is null)
        {
            return Enumerable.Empty<SemVersion>();
        }

        return package.Versions.Select(v => v.SemanticVersion);
    }

    public string? GetPackageName(string lowerName)
    {
        var package = this._dbContext.Packages.SingleOrDefault(p => p.LowerName == lowerName);

        return package?.Name;
    }
}
