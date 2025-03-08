using System.Threading.Tasks;
using Anna.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace Anna.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var services = builder.Services;

        services.Configure<AnnaClientOptions>(builder.Configuration.GetSection(nameof(AnnaClient)));
        services.AddHttpClient<IAnnaClient, AnnaClient>();

        services.AddMudServices();

        await builder.Build().RunAsync();
    }
}
