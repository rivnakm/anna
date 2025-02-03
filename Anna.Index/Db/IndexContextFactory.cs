using System;
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
        var conn = Environment.GetEnvironmentVariable(EnvironmentConstants.AnnaIndexDbConnectionString);

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
