using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Anna.Api.Extensions;
using Anna.Api.Resources;
using Anna.Common;
using Anna.Index;
using Anna.Index.Db;
using Anna.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Anna.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = null;
        });

        var services = builder.Services;

        services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddHealthChecks();

        services.AddDbContext<IndexContext>(options =>
        {
            var conn = EnvironmentUtility.GetRequiredEnvironmentVariable(Index.EnvironmentConstants.AnnaIndexDbConnectionString);
            options.UseNpgsql(conn);
        });

        services.AddSingleton<IResourceProvider, ResourceProvider>();

        services.AddScoped<IPackageIndex, PackageIndex>();
        services.AddScoped<IPackageStorage, PackageStorage>(_ => new PackageStorage(EnvironmentUtility.GetRequiredEnvironmentVariable(Storage.EnvironmentConstants.AnnaStorageRootDir)));

        // TODO: add exception middleware

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Anna API");
                options.RoutePrefix = "api";
            });

            app.UseDeveloperExceptionPage();
        }

        app.UseMigrations();
        app.UseRouting();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapHealthChecks("/healthcheck");

        app.MapControllers();

        app.Run();
    }
}
