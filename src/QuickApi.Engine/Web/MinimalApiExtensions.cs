using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Abstractions;
using QuickApi.Engine.Web.Endpoints;
using Scrutor;

namespace QuickApi.Engine.Web;

public static class MinimalApiExtensions
{
    public static IServiceCollection AddMinimalEndpoints(this IServiceCollection services, Action<MinimalApiOptions>? configure = null)
    {
        var options = new MinimalApiOptions();
        configure?.Invoke(options);
        services.Scan(scan =>
        {
            scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo<IMinimalEndpoint>()
                    .Where(type => !type.IsAbstract))
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IMinimalEndpoint>()
                .WithLifetime(options.ServiceLifetime);
        });
        
        foreach (var configuration in options.GetConfigurations())
            configuration(services);
        return services;
    }
    
    
    public static IEndpointRouteBuilder UseMinimalEndpoints(this IEndpointRouteBuilder builder)
    {
        var scope = builder.ServiceProvider.CreateScope();
        var endpoints = scope.ServiceProvider.GetServices<IMinimalEndpoint>();

        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(builder);

        return builder;
    }
}