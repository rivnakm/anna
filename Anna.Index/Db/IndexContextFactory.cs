using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Anna.Index.Db;

[ExcludeFromCodeCoverage]
public class IndexContextFactory : IDesignTimeDbContextFactory<IndexContext>
{
    public IndexContext CreateDbContext(string[] args)
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<IndexContext>();
        var dbPath = Environment.GetEnvironmentVariable(EnvironmentConstants.AnnaIndexDbPath)
            ?? throw new InvalidOperationException($"Environment variable '{EnvironmentConstants.AnnaIndexDbPath}' is not set");

        dbContextOptionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new IndexContext(dbContextOptionsBuilder.Options);
    }
}
