using Anna.Index.Db.Models;
using Microsoft.EntityFrameworkCore;
using Semver;
using Version = Anna.Index.Db.Models.Version;

namespace Anna.Index.Db;

public class IndexContext : DbContext
{
    public DbSet<Package> Packages { get; set; } = null!;
    public DbSet<Version> Versions { get; set; } = null!;

    public IndexContext(DbContextOptions<IndexContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Version>()
            .Property(e => e.SemanticVersion)
            .HasConversion(
                    v => v.ToString(),
                    v => SemVersion.Parse(v, SemVersionStyles.Strict, 1024)
            );
    }
}
