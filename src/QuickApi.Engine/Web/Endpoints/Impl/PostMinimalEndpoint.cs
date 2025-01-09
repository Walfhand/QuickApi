using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using QuickApi.Abstractions.Cqrs;
using QuickApi.Engine.Web.Endpoints.Enums;

namespace QuickApi.Engine.Web.Endpoints.Impl;

public abstract class PostMinimalEndpoint<TRequest, TResult>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint<TResult>(EndpointType.Post, path, policies)
    where TRequest : class
{
    protected override Delegate Handler => Endpoint;

    private static async Task<IResult> Endpoint([AsParameters] TRequest request, IMessage message,
        CancellationToken ct)
    {
        return Results.Created("", await message.InvokeAsync<TResult>(request, ct));
    }
}