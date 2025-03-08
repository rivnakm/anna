using Anna.Index.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Anna.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<IndexContext>();
        db.Database.Migrate();

        return app;
    }
}
