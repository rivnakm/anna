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
            throw new PackageNotFoundException();
        }

        return package.Versions.Select(v => v.PackageVersion);
    }

    public async Task<string> GetPackageName(string lowerName)
    {
        var package = await this._dbContext.Packages.SingleOrDefaultAsync(p => p.LowerName == lowerName);
        if (package is null)
        {
            throw new PackageNotFoundException();
        }

        return package.Name;
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

    public async Task UnlistPackage(string name, NuGetVersion version)
    {
        var packageVersion = (await this._dbContext.Packages.Include(p => p.Versions).SingleOrDefaultAsync(p => p.Name == name))?.Versions
            .SingleOrDefault(v => v.PackageVersion == version)
            ?? throw new PackageNotFoundException();

        packageVersion.Unlisted = true;

        await this._dbContext.SaveChangesAsync();
    }

    public async Task RelistPackage(string name, NuGetVersion version)
    {
        var packageVersion = (await this._dbContext.Packages.Include(p => p.Versions).SingleOrDefaultAsync(p => p.Name == name))?.Versions
            .SingleOrDefault(v => v.PackageVersion == version)
            ?? throw new PackageNotFoundException();

        packageVersion.Unlisted = false;

        await this._dbContext.SaveChangesAsync();
    }

    public async Task RemovePackage(string name, NuGetVersion version)
    {
        var package = await this._dbContext.Packages.Include(p => p.Versions).SingleOrDefaultAsync(p => p.Name == name);
        var packageVersion = package?.Versions
            .SingleOrDefault(v => v.PackageVersion == version)
            ?? throw new PackageNotFoundException();

        package.Versions.Remove(packageVersion);
        if (package.Versions.Count == 0)
        {
            this._dbContext.Packages.Remove(package);
        }

        await this._dbContext.SaveChangesAsync();
    }
}
