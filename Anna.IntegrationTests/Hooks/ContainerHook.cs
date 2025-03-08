using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anna.Client;
using Anna.IntegrationTests.Contexts;
using Anna.Test.Common;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Options;
using Reqnroll;
using Testcontainers.PostgreSql;

namespace Anna.IntegrationTests.Hooks;

[Binding]
public partial class ContainerHook
{
    private const ushort HttpPort = 8080;
    private static IImage? _image;

    [BeforeTestRun]
    public static void BuildContainer()
    {
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("Containerfile")
            .Build();

        Task.Run(() => image.CreateAsync()).Wait();
        _image = image;
    }

    [BeforeScenario]
    public async Task StartContainer(ContainerContext context, AnnaClientContext annaClientContext)
    {
        // Create network
        context.Network = new NetworkBuilder().Build();

        // Start PostgreSQL container
        const string dbContainerHostname = "anna-postgres";
        context.DbContainer = new PostgreSqlBuilder()
            .WithImage(TestConstants.PostgreSqlImage)
            .WithNetwork(context.Network)
            .WithHostname(dbContainerHostname)
            .WithWaitStrategy(Wait.ForUnixContainer()
                                  .AddCustomWaitStrategy(new PostgreSqlWaitStrategy(),
                                                         waitStrategyModifier: m =>
                                                             m.WithTimeout(TimeSpan.FromMinutes(1))))
            .Build();
        await context.DbContainer.StartAsync();

        // Connection string from the host isn't the same as what it would be from another container
        // same for the port
        // context.DbContainer.Hostname also doesn't return the container's hostname (bug?)
        var connString = ConnectionStringHostRegex()
            .Replace(context.DbContainer.GetConnectionString(), $"Host={dbContainerHostname};").Trim();
        connString = ConnectionStringPortRegex().Replace(connString, "Port=5432;").Trim();

        // Start API container
        context.ApiContainer = new ContainerBuilder()
            .WithImage(_image)
            .WithImagePullPolicy(PullPolicy.Never)
            .WithEnvironment("ConnectionStrings__Index", connString)
            .WithNetwork(context.Network)
            .WithPortBinding(HttpPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(
                              new BoundPortHttpRequestWaitStrategy(HttpPort, "/healthcheck"),
                              waitStrategyModifier: w => w.WithTimeout(TimeSpan.FromMinutes(1))))
            .Build();

        await context.ApiContainer.StartAsync();

        var clientOptions = new AnnaClientOptions
        {
            IndexUrl = new UriBuilder("http",
                                      context.ApiContainer.Hostname,
                                      context.ApiContainer.GetMappedPublicPort(HttpPort),
                                      "v3/index.json").Uri
        };

        annaClientContext.AnnaClient =
            new AnnaClient(new HttpClient(), new OptionsWrapper<AnnaClientOptions>(clientOptions));
    }

    [AfterScenario]
    public async Task StopContainer(ContainerContext context)
    {
        if (context.ApiContainer is not null)
        {
            var (stdout, stderr) = await context.ApiContainer.GetLogsAsync();
            await context.ApiContainer.StopAsync();

            var timestamp = DateTime.Now.ToString("o");
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            var logsDirectory = Path.Combine(projectDirectory, "logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            await File.WriteAllTextAsync(Path.Combine(logsDirectory, $"api-container-{timestamp}.out.txt"), stdout);
            await File.WriteAllTextAsync(Path.Combine(logsDirectory, $"api-container-{timestamp}.err.txt"), stderr);
        }

        if (context.DbContainer is not null)
        {
            await context.DbContainer.StopAsync();
        }
    }

    [GeneratedRegex("Host=(.*?);")]
    private static partial Regex ConnectionStringHostRegex();

    [GeneratedRegex("Port=(.*?);")]
    private static partial Regex ConnectionStringPortRegex();
}
