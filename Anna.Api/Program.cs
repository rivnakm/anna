using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Anna.Api.Conventions;
using Anna.Index;
using Anna.Index.Db;
using Microsoft.AspNetCore.Builder;
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
        var services = builder.Services;

        services.AddControllers(options =>
            {
                options.Conventions.Add(new ResourceAttributeMetadataConvention());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDbContext<IndexContext>(options =>
        {
            var dbPath = Environment.GetEnvironmentVariable(EnvironmentConstants.AnnaIndexDbPath) ?? throw new InvalidOperationException($"Environment variable '{EnvironmentConstants.AnnaIndexDbPath}' is not set");

            options.UseSqlite($"Data Source={dbPath}");
        });

        services.AddScoped<IPackageIndex, PackageIndex>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
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

        app.UseRouting();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}