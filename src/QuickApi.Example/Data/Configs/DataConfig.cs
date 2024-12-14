using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuickApi.Example.Data.Contexts;

namespace QuickApi.Example.Data.Configs;

internal static class DataConfig
{
    public static IServiceCollection AddData(this IServiceCollection services)
    {
        services.AddOptions<PostgresOptions>()
            .BindConfiguration(PostgresOptions.Postgres)
            .ValidateDataAnnotations();
        
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var postgresOptions = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
            options.UseNpgsql(postgresOptions.ConnectionString, dbOptions =>
            {
                dbOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
                dbOptions.EnableRetryOnFailure();
            }).UseSnakeCaseNamingConvention();
        });
        services.AddScoped<IDbContext, AppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        return services;
    }

    public static IApplicationBuilder UseData(this IApplicationBuilder applicationBuilder)
    {
        MigrateAsync(applicationBuilder).GetAwaiter().GetResult();
        return applicationBuilder;
    }

    private static async Task MigrateAsync(this IApplicationBuilder applicationBuilder)
    {
        using var scope = applicationBuilder.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await context.Database.MigrateAsync();
    }
}