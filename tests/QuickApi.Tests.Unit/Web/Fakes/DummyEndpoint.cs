using Microsoft.AspNetCore.Routing;
using QuickApi.Engine.Web.Endpoints;

namespace QuickApi.Tests.Unit.Web.Fakes;

public class DummyEndpoint : IMinimalEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        // no-op for registration verification
    }
}
