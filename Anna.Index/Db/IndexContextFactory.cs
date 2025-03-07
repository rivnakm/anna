using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Anna.Index.Db;

[ExcludeFromCodeCoverage]
public class IndexContextFactory : IDesignTimeDbContextFactory<IndexContext>
{
    public IndexContext CreateDbContext(string[] args)
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<IndexContext>();
        var config = new ConfigurationBuilder().AddEnvironmentVariables().AddUserSecrets<IndexContext>().Build();
        var conn = config.GetConnectionString("Index");

        if (conn is null)
        {
            // We don't need a connection to run `dotnet ef migrations ...`, but we do need to know the database provider
            dbContextOptionsBuilder.UseNpgsql();
        }
        else
        {
            dbContextOptionsBuilder.UseNpgsql(conn);
        }

        return new IndexContext(dbContextOptionsBuilder.Options);
    }
}
