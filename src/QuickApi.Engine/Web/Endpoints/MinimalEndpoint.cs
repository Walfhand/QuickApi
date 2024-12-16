using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using QuickApi.Engine.Web.Endpoints.Enums;
using QuickApi.Engine.Web.Models;

namespace QuickApi.Engine.Web.Endpoints;

public abstract class MinimalEndpoint(
    EndpointType endpointType,
    string path,
    params string[] policies)
    : MinimalEndpoint<object>(endpointType, path, policies);

public abstract class MinimalEndpoint<TResult> : IMinimalEndpoint
{
    private readonly EndpointType _endpointType;
    private readonly string _path;
    private readonly string[] _policies;
    
    protected abstract Delegate Handler { get; }

    protected virtual RouteHandlerBuilder Configure(IEndpointRouteBuilder builder)
    {
        var routeBuilder = CreateEndpoint<TResult>(builder,_endpointType, _path, Handler);
        if (_policies.Length != 0)
            routeBuilder.RequireAuthorization(_policies);
        return routeBuilder;
    }
    
    protected MinimalEndpoint(EndpointType endpointType, string path, params string[] policies)
    {
        _endpointType = endpointType;
        _path = path;
        _policies = policies;
    }
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        Configure(builder);
    }
    
    private static RouteHandlerBuilder CreateEndpoint<TEntity>(IEndpointRouteBuilder builder,
        EndpointType endpointType, [StringSyntax("Route")] string pattern, Delegate handler)
    {
        var context = pattern.Split('/').First();
        return CreateConvention<TEntity>(builder,endpointType, $"{EndpointPath.BaseApiPath}/{pattern}", handler)
            .WithTags(context);
    }
    
    private static RouteHandlerBuilder CreateConvention<TEntity>(IEndpointRouteBuilder builder,
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