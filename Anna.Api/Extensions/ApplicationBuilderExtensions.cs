using System;
using Anna.Index.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;
using Polly.Retry;

namespace Anna.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    private static readonly ResiliencePipeline Pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<PostgresException>(),
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        })
        .Build();

    public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
    {
        Pipeline.Execute(_ => {
            using var scope = app.ApplicationServices.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<IndexContext>();
            db.Database.Migrate();
        });

        return app;
    }
}
