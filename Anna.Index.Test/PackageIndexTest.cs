using System;
using System.Collections.Generic;
using System.Linq;
using Anna.Index.Db;
using Anna.Index.Db.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Semver;
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
    public void TestGetVersions_NoMatch_ReturnsEmpty()
    {
        var versions = this._packageIndex.GetVersions("package");
        versions.Should().BeEmpty();
    }

    [Fact]
    public void TestGetVersions()
    {
        var package = new Package
        {
            Name = "Package",
            LowerName = "package",
            Versions = new List<Version>
        {
            new Version {
                SemanticVersion = new SemVersion(1, 0, 0),
            },
            new Version {
                SemanticVersion = new SemVersion(2, 0, 0),
            }
        }
        };

        this._dbContext.Add(package);
        this._dbContext.SaveChanges();

        var versions = this._packageIndex.GetVersions("package");
        versions.Should().BeEquivalentTo(package.Versions.Select(v => v.SemanticVersion));
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
