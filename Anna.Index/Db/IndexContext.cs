using Anna.Index.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using Version = Anna.Index.Models.Version;

namespace Anna.Index.Db;

public class IndexContext : DbContext
{
    internal DbSet<Package> Packages { get; set; } = null!;
    internal DbSet<Version> Versions { get; set; } = null!;

    public IndexContext(DbContextOptions<IndexContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Version>()
            .Property(e => e.PackageVersion)
            .HasConversion(
                    v => v.ToString(),
                    v => NuGetVersion.Parse(v)
            );
    }
}
