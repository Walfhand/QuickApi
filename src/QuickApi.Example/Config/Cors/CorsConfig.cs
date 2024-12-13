namespace QuickApi.Example.Config.Cors;

public static class CorsConfig
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("default",
                builder =>
                {
                    builder.WithOrigins("*")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        return services;
    }

    public static WebApplication UseCustomCors(this WebApplication app)
    {
        app.UseCors("default");
        return app;
    }
}