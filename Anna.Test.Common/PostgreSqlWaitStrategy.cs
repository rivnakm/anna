using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Anna.Test.Common;

public class PostgreSqlWaitStrategy : IWaitUntil
{
    public async Task<bool> UntilAsync(IContainer container)
    {
        if (container is not PostgreSqlContainer pgContainer)
        {
            throw new InvalidOperationException($"{nameof(PostgreSqlWaitStrategy)} is only supported for PostgreSQL containers");
        }

        try
        {
            await using var conn = new NpgsqlConnection(pgContainer!.GetConnectionString());
            await conn.OpenAsync();
            return true;
        }
        catch (PostgresException e)
        {
            if (e.SqlState == PostgresErrorCodes.CannotConnectNow)
            {
                return false;
            }
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
