using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anna.Index.Db;
using Anna.Index.Db.Models;
using Anna.Index.Exceptions;
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
        var get = async () => await this._packageIndex.GetVersions("package");

        await get.Should().ThrowAsync<PackageNotFoundException>();
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
    public async Task TestGetPackageName_NoMatch_Throws()
    {
        var get = async () => await this._packageIndex.GetPackageName("package");

        await get.Should().ThrowAsync<PackageNotFoundException>();
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

    [Fact]
    public async Task TestAddPackage_NewPackage()
    {
        const string packageName = "Package";
        var packageVersion = new NuGetVersion(1, 0, 0);

        await this._packageIndex.AddPackage(packageName, packageVersion);

        this._dbContext.Packages.Should().ContainSingle(p => p.Name == packageName && p.LowerName == packageName.ToLowerInvariant());

        var package = await this._dbContext.Packages.Include(p => p.Versions).SingleAsync(p => p.Name == packageName);
        package.Versions.Should().ContainSingle(v => v.PackageVersion == packageVersion);
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

        var newVersion = new NuGetVersion(3, 0, 0);

        await this._packageIndex.AddPackage(package.Name, newVersion);

        this._dbContext.Packages.Should().ContainSingle(p => p.Name == package.Name && p.LowerName == package.LowerName);

        var outPackage = await this._dbContext.Packages.Include(p => p.Versions).SingleAsync(p => p.Name == package.Name);
        package.Versions.Should().ContainSingle(v => v.PackageVersion == newVersion);
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

        var newVersion = new NuGetVersion(1, 0, 0);

        var add = async () => await this._packageIndex.AddPackage(package.Name, newVersion);

        await add.Should().ThrowAsync<PackageExistsException>();
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
            new Version {
                PackageVersion = packageVersion,
            },
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.UnlistPackage(packageName, packageVersion);

        var pkgVerObject = this._dbContext.Packages.Include(p => p.Versions).Single(p => p.Name == packageName).Versions.Single(v => v.PackageVersion == packageVersion);

        pkgVerObject.Unlisted.Should().BeTrue();
    }

    [Fact]
    public async Task TestUnlist_NoNameMatch_Throws()
    {
        var unlist = async () => await this._packageIndex.UnlistPackage("package", new NuGetVersion(1, 0, 0));

        await unlist.Should().ThrowAsync<PackageNotFoundException>();
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
            new Version {
                PackageVersion = new NuGetVersion(1, 0, 0)
            },
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var unlist = async () => await this._packageIndex.UnlistPackage(packageName, new NuGetVersion(2, 0, 0));

        await unlist.Should().ThrowAsync<PackageNotFoundException>();
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
            new Version {
                PackageVersion = packageVersion,
                Unlisted = true,
            },
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.RelistPackage(packageName, packageVersion);

        var pkgVerObject = this._dbContext.Packages.Include(p => p.Versions).Single(p => p.Name == packageName).Versions.Single(v => v.PackageVersion == packageVersion);

        pkgVerObject.Unlisted.Should().BeFalse();
    }

    [Fact]
    public async Task TestRelist_NoNameMatch_Throws()
    {
        var relist = async () => await this._packageIndex.RelistPackage("package", new NuGetVersion(1, 0, 0));

        await relist.Should().ThrowAsync<PackageNotFoundException>();
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
            new Version {
                PackageVersion = new NuGetVersion(1, 0, 0)
            },
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var relist = async () => await this._packageIndex.RelistPackage(packageName, new NuGetVersion(2, 0, 0));

        await relist.Should().ThrowAsync<PackageNotFoundException>();
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
            new Version {
                PackageVersion = packageVersion,
            },
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.RemovePackage(packageName, packageVersion);

        this._dbContext.Packages.Should().NotContain(p => p.Name == packageName);
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
            new Version {
                PackageVersion = packageVersion,
            },
            new Version {
                PackageVersion = existingVersion,
            }
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        await this._packageIndex.RemovePackage(packageName, packageVersion);

        this._dbContext.Packages.Should().Contain(p => p.Name == packageName);

        var outPackage = this._dbContext.Packages.Include(p => p.Versions).Single(p => p.Name == packageName);
        outPackage.Versions.Should().ContainSingle(v => v.PackageVersion == existingVersion);
        outPackage.Versions.Should().NotContain(v => v.PackageVersion == packageVersion);
    }

    [Fact]
    public async Task TestRemovePackage_NoNameMatch_Throws()
    {
        var remove = async () => await this._packageIndex.RemovePackage("package", new NuGetVersion(1, 0, 0));

        await remove.Should().ThrowAsync<PackageNotFoundException>();
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
            new Version {
                PackageVersion = new NuGetVersion(1, 0, 0)
            },
        }
        };

        await this._dbContext.AddAsync(package);
        await this._dbContext.SaveChangesAsync();

        var remove = async () => await this._packageIndex.RemovePackage(packageName, new NuGetVersion(2, 0, 0));

        await remove.Should().ThrowAsync<PackageNotFoundException>();
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
