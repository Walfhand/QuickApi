using Microsoft.AspNetCore.Routing;

namespace QuickApi.Engine.Web.Endpoints;

public interface IMinimalEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder);
}