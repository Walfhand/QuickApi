using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using QuickApi.Engine.Web.Cqrs;
using QuickApi.Engine.Web.Endpoints.Enums;

namespace QuickApi.Engine.Web.Endpoints.Impl;

public abstract class DeleteMinimalEndpoint<TRequest>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint(EndpointType.Delete, path, policies)
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