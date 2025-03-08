using Anna.Index.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using Version = Anna.Index.Models.Version;

namespace Anna.Index.Db;

public class IndexContext : DbContext
{

    public IndexContext(DbContextOptions<IndexContext> options) : base(options)
    {
    }

    internal DbSet<Package> Packages { get; set; } = null!;
    internal DbSet<Version> Versions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Version>()
            .Property(e => e.PackageVersion)
            .HasConversion(
            convertToProviderExpression: v => v.ToString(),
            convertFromProviderExpression: v => NuGetVersion.Parse(v)
            );
    }
}
