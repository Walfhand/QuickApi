using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using QuickApi.Engine.Web.Cqrs;
using QuickApi.Engine.Web.Endpoints.Enums;

namespace QuickApi.Engine.Web.Endpoints.Impl;

public abstract class PostMinimalEndpoint<TRequest, TResult>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint<TResult>(EndpointType.Post, path, policies)
    where TRequest : class
{
    protected override Delegate Handler => Endpoint;

    private static async Task<IResult> Endpoint([AsParameters] TRequest request, IMessage bus,
        CancellationToken ct)
    {
        return Results.Created("", await bus.InvokeAsync<TResult>(request, ct));
    }
}