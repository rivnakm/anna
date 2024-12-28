using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anna.Index.Db;
using Anna.Index.Db.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using Version = Anna.Index.Db.Models.Version;

namespace Anna.Index.Test;

public class PackageIndexTest : IDisposable
{
    private IndexContext _dbContext;
    private PackageIndex _packageIndex;

    public PackageIndexTest()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<IndexContext>();
        dbContextOptionsBuilder.UseSqlite("Data Source=:memory:;");

        this._dbContext = new IndexContext(dbContextOptionsBuilder.Options);
        this._dbContext.Database.OpenConnection();
        this._dbContext.Database.Migrate();

        this._packageIndex = new PackageIndex(this._dbContext);
    }

    [Fact]
    public async Task TestGetVersions_NoMatch_ReturnsEmpty()
    {
        var versions = await this._packageIndex.GetVersions("package");
        versions.Should().BeEmpty();
    }

    [Fact]
    public async Task TestGetVersions()
    {
        var package = new Package
        {
            Name = "Package",
            LowerName = "package",
            Versions = new List<Version>
        {
            new Version {
                PackageVersion = new NuGetVersion(1, 0, 0),
            },
            new Version {
                PackageVersion = new NuGetVersion(2, 0, 0),
            }
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var versions = await this._packageIndex.GetVersions("package");
        versions.Should().BeEquivalentTo(package.Versions.Select(v => v.PackageVersion));
    }

    [Fact]
    public async Task TestGetPackageName_NoMatch_ReturnsNull()
    {
        var versions = await this._packageIndex.GetPackageName("package");
        versions.Should().BeNull();
    }

    [Fact]
    public async Task TestGetPackageName()
    {
        var package = new Package
        {
            Name = "Package",
            LowerName = "package",
            Versions = new List<Version>
        {
            new Version {
                PackageVersion = new NuGetVersion(1, 0, 0),
            },
            new Version {
                PackageVersion = new NuGetVersion(2, 0, 0),
            }
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var name = await this._packageIndex.GetPackageName(package.LowerName);
        name.Should().Be(package.Name);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._dbContext.Dispose();
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
