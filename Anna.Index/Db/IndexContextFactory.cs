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
        var dbPath = Environment.GetEnvironmentVariable(EnvironmentConstants.AnnaIndexDbPath);

        if (dbPath is null)
        {
            // We don't need a connection to run `dotnet ef migrations ...`, but we do need to know the database provider
            dbContextOptionsBuilder.UseSqlite();
        }
        else
        {
            dbContextOptionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        return new IndexContext(dbContextOptionsBuilder.Options);
    }
}
