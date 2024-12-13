using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using QuickApi.Engine.Web.Cqrs;
using QuickApi.Engine.Web.Endpoints.Enums;

namespace QuickApi.Engine.Web.Endpoints.Impl;

public abstract class GetMinimalEndpoint<TRequest, TResult>([StringSyntax("Route")] string path,
    params string[] policies) : MinimalEndpoint<TResult>(EndpointType.Get, path, policies)
    where TRequest : class
{
    protected override Delegate Handler => GetEndpoint;
    
    private static async Task<IResult> GetEndpoint([AsParameters] TRequest request, IMessage message,
        CancellationToken ct)
        => Results.Ok(await message.InvokeAsync<TResult>(request, ct));
}