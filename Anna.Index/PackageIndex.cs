using Anna.Index.Db;
using Anna.Index.Exceptions;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using Version = Anna.Index.Db.Models.Version;

namespace Anna.Index;

public class PackageIndex : IPackageIndex
{
    private readonly IndexContext _dbContext;

    public PackageIndex(IndexContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<IEnumerable<NuGetVersion>> GetVersions(string lowerName)
    {
        var package = await this._dbContext.Packages.Include(p => p.Versions).SingleOrDefaultAsync(p => p.LowerName == lowerName);
        if (package is null)
        {
            return Enumerable.Empty<NuGetVersion>();
        }

        return package.Versions.Select(v => v.PackageVersion);
    }

    public async Task<string?> GetPackageName(string lowerName)
    {
        var package = await this._dbContext.Packages.SingleOrDefaultAsync(p => p.LowerName == lowerName);

        return package?.Name;
    }

    public async Task AddPackage(string name, NuGetVersion version)
    {
        var package = await this._dbContext.Packages.Include(p => p.Versions).SingleOrDefaultAsync(p => p.Name == name);
        if (package is null)
        {
            package = (await this._dbContext.AddAsync(new Db.Models.Package
            {
                Name = name,
                LowerName = name.ToLowerInvariant(),
                Versions = new List<Version>()
            })).Entity;
        }

        if (package.Versions.Any(v => v.PackageVersion == version))
        {
            throw new PackageExistsException();
        }

        package.Versions.Add(new Version
        {
            PackageVersion = version
        });

        await this._dbContext.SaveChangesAsync();
    }

    public Task UnlistPackage(string name, NuGetVersion version)
    {
        throw new NotImplementedException();
    }

    public Task RelistPackage(string name, NuGetVersion version)
    {
        throw new NotImplementedException();
    }

    public Task RemovePackage(string name, NuGetVersion version)
    {
        throw new NotImplementedException();
    }
}
