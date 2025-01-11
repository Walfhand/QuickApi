using QuickApi.Engine.Web.Endpoints.Impl;

namespace QuickApi.Tests.Unit.Endpoints;

public record GetEndpointRequest();
public record GetEndpointResult();

public class GetEndpoint() : GetMinimalEndpoint<GetEndpointRequest, GetEndpointResult>("api/tests")
{
    
}