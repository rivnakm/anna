using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anna.Index.Db;
using Anna.Index.Exceptions;
using Anna.Index.Models;
using Anna.Test.Common;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using Shouldly;
using Testcontainers.PostgreSql;
using Version = Anna.Index.Models.Version;

namespace Anna.Index.Test;

public class PackageIndexTest : IAsyncLifetime
{
    private IndexContext _dbContext = null!;
    private PackageIndex _packageIndex = null!;
    private PostgreSqlContainer _pgContainer = null!;

    public async Task InitializeAsync()
    {
        this._pgContainer = new PostgreSqlBuilder()
            .WithImage(TestConstants.PostgreSqlImage)
            .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(
                              new PostgreSqlWaitStrategy(),
                              waitStrategyModifier: o => o.WithTimeout(TimeSpan.FromMinutes(1)))).Build();
        await this._pgContainer.StartAsync();
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<IndexContext>();
        dbContextOptionsBuilder.UseNpgsql(this._pgContainer.GetConnectionString());

        this._dbContext = new IndexContext(dbContextOptionsBuilder.Options);
        await this._dbContext.Database.OpenConnectionAsync();
        await this._dbContext.Database.MigrateAsync();

        this._packageIndex = new PackageIndex(this._dbContext);
    }

    public async Task DisposeAsync()
    {
        await this._pgContainer.DisposeAsync();
    }

    [Fact]
    public async Task TestGetVersions_NoMatch_ReturnsEmpty()
    {
        await Should.ThrowAsync<PackageNotFoundException>(
        async () => await this._packageIndex.GetVersions("package"));
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
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                },
                new()
                {
                    PackageVersion = new NuGetVersion(2, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var versions = (await this._packageIndex.GetVersions("package")).ToList();
        versions.ShouldBeEquivalentTo(package.Versions.Select(v => v.PackageVersion).ToList());
    }

    [Fact]
    public async Task TestGetVersions_MatchesOnly()
    {
        var package = new Package
        {
            Name = "Package",
            LowerName = "package",
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                }
            }
        };
        var packageB = new Package
        {
            Name = "PackageB",
            LowerName = "packageB",
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = new NuGetVersion(2, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.AddAsync(packageB);
        await this._dbContext.SaveChangesAsync();

        var versions = (await this._packageIndex.GetVersions("package")).ToList();
        versions.ShouldBeEquivalentTo(package.Versions.Select(v => v.PackageVersion).ToList());
    }

    [Fact]
    public async Task TestGetPackageName_NoMatch_Throws()
    {
        await Should.ThrowAsync<PackageNotFoundException>(
        async () => await this._packageIndex.GetPackageName("package"));
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
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                },
                new()
                {
                    PackageVersion = new NuGetVersion(2, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var name = await this._packageIndex.GetPackageName(package.LowerName);
        name.ShouldBe(package.Name);
    }

    [Fact]
    public async Task TestAddPackage_NewPackage()
    {
        const string packageName = "Package";
        var packageVersion = new NuGetVersion(1, 0, 0);

        await this._packageIndex.AddPackage(packageName, packageVersion);

        this._dbContext.Packages.ShouldContain(p => p.Name == packageName &&
                                                    p.LowerName == packageName.ToLowerInvariant());

        var package = await this._dbContext.Packages.Include(p => p.Versions).SingleAsync(p => p.Name == packageName);
        package.Versions.ShouldContain(v => v.PackageVersion == packageVersion);
    }

    [Fact]
    public async Task TestAddPackage_ExistingPackageNewVersion()
    {
        var package = new Package
        {
            Name = "Package",
            LowerName = "package",
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                },
                new()
                {
                    PackageVersion = new NuGetVersion(2, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var newVersion = new NuGetVersion(3, 0, 0);

        await this._packageIndex.AddPackage(package.Name, newVersion);

        this._dbContext.Packages.ShouldContain(p => p.Name == package.Name && p.LowerName == package.LowerName);

        var outPackage = await this._dbContext.Packages.Include(p => p.Versions)
            .SingleAsync(p => p.Name == package.Name);
        package.Versions.ShouldContain(v => v.PackageVersion == newVersion);
    }

    [Fact]
    public async Task TestAddPackage_ExistingPackageExistingVersion_Throws()
    {
        var package = new Package
        {
            Name = "Package",
            LowerName = "package",
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                },
                new()
                {
                    PackageVersion = new NuGetVersion(2, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var newVersion = new NuGetVersion(1, 0, 0);

        var add = async () => await this._packageIndex.AddPackage(package.Name, newVersion);

        await add.ShouldThrowAsync<PackageExistsException>();
    }

    [Fact]
    public async Task TestUnlistPackage()
    {
        const string packageName = "Package";
        var packageVersion = new NuGetVersion(1, 0, 0);
        var package = new Package
        {
            Name = packageName,
            LowerName = packageName.ToLowerInvariant(),
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = packageVersion
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.UnlistPackage(packageName, packageVersion);

        var pkgVerObject = this._dbContext.Packages.Include(p => p.Versions).Single(p => p.Name == packageName).Versions
            .Single(v => v.PackageVersion == packageVersion);

        pkgVerObject.Unlisted.ShouldBeTrue();
    }

    [Fact]
    public async Task TestUnlist_NoNameMatch_Throws()
    {
        var unlist = async () => await this._packageIndex.UnlistPackage("package", new NuGetVersion(1, 0, 0));

        await unlist.ShouldThrowAsync<PackageNotFoundException>();
    }

    [Fact]
    public async Task TestUnlistPackage_NoVersionMatch_Throws()
    {
        const string packageName = "Package";
        var package = new Package
        {
            Name = packageName,
            LowerName = packageName.ToLowerInvariant(),
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var unlist = async () => await this._packageIndex.UnlistPackage(packageName, new NuGetVersion(2, 0, 0));

        await unlist.ShouldThrowAsync<PackageNotFoundException>();
    }

    [Fact]
    public async Task TestRelistPackage()
    {
        const string packageName = "Package";
        var packageVersion = new NuGetVersion(1, 0, 0);
        var package = new Package
        {
            Name = packageName,
            LowerName = packageName.ToLowerInvariant(),
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = packageVersion,
                    Unlisted = true
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.RelistPackage(packageName, packageVersion);

        var pkgVerObject = this._dbContext.Packages.Include(p => p.Versions).Single(p => p.Name == packageName).Versions
            .Single(v => v.PackageVersion == packageVersion);

        pkgVerObject.Unlisted.ShouldBeFalse();
    }

    [Fact]
    public async Task TestRelist_NoNameMatch_Throws()
    {
        var relist = async () => await this._packageIndex.RelistPackage("package", new NuGetVersion(1, 0, 0));

        await relist.ShouldThrowAsync<PackageNotFoundException>();
    }

    [Fact]
    public async Task TestRelistPackage_NoVersionMatch_Throws()
    {
        const string packageName = "Package";
        var package = new Package
        {
            Name = packageName,
            LowerName = packageName.ToLowerInvariant(),
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var relist = async () => await this._packageIndex.RelistPackage(packageName, new NuGetVersion(2, 0, 0));

        await relist.ShouldThrowAsync<PackageNotFoundException>();
    }

    [Fact]
    public async Task TestRemovePackage_OnlyVersion_RemovesPackage()
    {
        const string packageName = "Package";
        var packageVersion = new NuGetVersion(1, 0, 0);
        var package = new Package
        {
            Name = packageName,
            LowerName = packageName.ToLowerInvariant(),
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = packageVersion
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.RemovePackage(packageName, packageVersion);

        this._dbContext.Packages.ShouldNotContain(p => p.Name == packageName);
    }

    [Fact]
    public async Task TestRemovePackage_MultipleVersions_RetainsPackage()
    {
        const string packageName = "Package";
        var existingVersion = new NuGetVersion(2, 0, 0);
        var packageVersion = new NuGetVersion(1, 0, 0);
        var package = new Package
        {
            Name = packageName,
            LowerName = packageName.ToLowerInvariant(),
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = packageVersion
                },
                new()
                {
                    PackageVersion = existingVersion
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.RemovePackage(packageName, packageVersion);

        this._dbContext.Packages.ShouldContain(p => p.Name == packageName);

        var outPackage = this._dbContext.Packages.Include(p => p.Versions).Single(p => p.Name == packageName);
        outPackage.Versions.ShouldContain(v => v.PackageVersion == existingVersion);
        outPackage.Versions.ShouldNotContain(v => v.PackageVersion == packageVersion);
    }

    [Fact]
    public async Task TestRemovePackage_NoNameMatch_Throws()
    {
        await Should.ThrowAsync<PackageNotFoundException>(
        async () => await this._packageIndex.RemovePackage("package", new NuGetVersion(1, 0, 0)));
    }

    [Fact]
    public async Task TestRemovePackage_NoVersionMatch_Throws()
    {
        const string packageName = "Package";
        var package = new Package
        {
            Name = packageName,
            LowerName = packageName.ToLowerInvariant(),
            Versions = new List<Version>
            {
                new()
                {
                    PackageVersion = new NuGetVersion(1, 0, 0)
                }
            }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await Should.ThrowAsync<PackageNotFoundException>(
        async () => await this._packageIndex.RemovePackage(packageName, new NuGetVersion(2, 0, 0)));
    }
}
