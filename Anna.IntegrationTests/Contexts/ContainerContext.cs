using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace Anna.IntegrationTests.Contexts;

public class ContainerContext
{
    public INetwork? Network { get; set; }
    public PostgreSqlContainer? DbContainer { get; set; }
    public IContainer? ApiContainer { get; set; }
}
