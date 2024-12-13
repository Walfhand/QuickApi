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
    
    
    public static RouteHandlerBuilder CreateEndpoint<TEntity>(this IEndpointRouteBuilder builder,
        EndpointType endpointType, [StringSyntax("Route")] string pattern, Delegate handler)
    {
        var context = pattern.Split('/').First();
        return builder.CreateConvention<TEntity>(endpointType, $"{EndpointPath.BaseApiPath}/{pattern}", handler)
            .WithTags(context);
    }
    
    private static RouteHandlerBuilder CreateConvention<TEntity>(this IEndpointRouteBuilder builder,
        EndpointType endpointType, string path, Delegate handler)
        => endpointType switch
        {
            EndpointType.FilterPaginate => builder.MapGet(path, handler)
                .Produces<PaginatedResult<TEntity>>(),
            EndpointType.Filter => builder.MapGet(path, handler)
                .Produces<List<TEntity>>(),
            EndpointType.Get => builder.MapGet(path, handler)
                .Produces<TEntity>()
                .ProducesProblem(404),
            EndpointType.Post => builder.MapPost(path, handler)
                .Produces<TEntity>(201)
                .ProducesValidationProblem(),
            EndpointType.Put => builder.MapPut(path, handler)
                .Produces(204)
                .ProducesProblem(404)
                .ProducesValidationProblem(),
            EndpointType.Patch => builder.MapPatch(path, handler).Produces(204)
                .ProducesProblem(404)
                .ProducesValidationProblem(),
            EndpointType.Delete => builder.MapDelete(path, handler).Produces(204)
                .ProducesProblem(404)
                .ProducesValidationProblem(),
            _ => throw new NotImplementedException()
        };
}