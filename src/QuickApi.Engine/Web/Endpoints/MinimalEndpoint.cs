using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using QuickApi.Engine.Web.Endpoints.Enums;

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

    protected virtual void Configure(RouteHandlerBuilder routeBuilder)
    {
        
    }
    
    protected MinimalEndpoint(EndpointType endpointType, string path, params string[] policies)
    {
        _endpointType = endpointType;
        _path = path;
        _policies = policies;
    }
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var routeBuilder = builder.CreateEndpoint<TResult>(_endpointType, _path, Handler);
        Configure(routeBuilder);
        if (_policies.Length != 0)
            routeBuilder.RequireAuthorization(_policies);
    }
}