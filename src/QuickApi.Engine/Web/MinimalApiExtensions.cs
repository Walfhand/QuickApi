using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Engine.Web.Endpoints;
using QuickApi.Engine.Web.Endpoints.Enums;
using QuickApi.Engine.Web.Models;
using Scrutor;

namespace QuickApi.Engine.Web;

public static class MinimalApiExtensions
{
    public static IServiceCollection AddMinimalEndpoints(this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        services.Scan(scan =>
        {
            scan.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(classes => classes.AssignableTo<IMinimalEndpoint>()
                    .Where(type => !type.IsAbstract))
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IMinimalEndpoint>()
                .WithLifetime(serviceLifetime);
        });

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