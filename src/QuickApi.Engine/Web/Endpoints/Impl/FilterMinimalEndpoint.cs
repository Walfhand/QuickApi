using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using QuickApi.Abstractions.Cqrs;
using QuickApi.Engine.Web.Endpoints.Enums;
using QuickApi.Engine.Web.Models;

namespace QuickApi.Engine.Web.Endpoints.Impl;

public abstract class FilterMinimalEndpoint<TRequest, TResult>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint<TResult>(EndpointType.Filter, path, policies)
    where TRequest : class
{
    protected override Delegate Handler => EndPoint;

    private static async Task<IResult> EndPoint([AsParameters] TRequest request, IMessage bus,
        CancellationToken ct)
    {
        return Results.Ok(await bus.InvokeAsync<List<TResult>>(request, ct));
    }
}


public abstract class FilterPaginateMinimalEndpoint<TRequest, TResult>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint<TResult>(EndpointType.FilterPaginate, path, policies)
    where TRequest : class
{
    protected override Delegate Handler => EndPoint;

    private static async Task<IResult> EndPoint([AsParameters] TRequest request, IMessage bus,
        CancellationToken ct)
    {
        return Results.Ok(await bus.InvokeAsync<PaginatedResult<TResult>>(request, ct));
    }
}