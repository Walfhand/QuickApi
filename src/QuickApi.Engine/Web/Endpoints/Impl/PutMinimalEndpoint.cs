using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using QuickApi.Engine.Web.Cqrs;
using QuickApi.Engine.Web.Endpoints.Enums;

namespace QuickApi.Engine.Web.Endpoints.Impl;

public abstract class PatchMinimalEndpoint<TRequest>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint(EndpointType.Patch, path, policies)
    where TRequest : class
{
    protected override Delegate Handler => EndPoint;

    private static async Task<IResult> EndPoint([AsParameters] TRequest request, IMessage message,
        CancellationToken ct)
    {
        await message.InvokeAsync(request, ct);
        return Results.NoContent();
    }
}


public abstract class PutMinimalEndpoint<TRequest>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint(EndpointType.Put, path, policies)
    where TRequest : class
{
    protected override Delegate Handler => EndPoint;

    private static async Task<IResult> EndPoint([AsParameters] TRequest request, IMessage message,
        CancellationToken ct)
    {
        await message.InvokeAsync(request, ct);
        return Results.NoContent();
    }
}