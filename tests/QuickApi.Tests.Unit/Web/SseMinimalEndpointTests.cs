using System.Runtime.CompilerServices;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Engine.Web;
using QuickApi.Engine.Web.Endpoints;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace QuickApi.Tests.Unit.Web;

public class SseMinimalEndpointTests
{
    [Fact]
    public async Task SseEndpoint_ShouldReturnEventStreamContent()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IMinimalEndpoint, FakeSseEndpoint>();
        var app = builder.Build();
        app.UseMinimalEndpoints();
        await app.StartAsync();

        var client = app.GetTestClient();
        var response = await client.GetAsync("/api/test/sse");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/event-stream");
        content.Should().Contain("event: todo-updated");
        content.Should().Contain("data: {\"message\":\"hello\"}");
    }

    private sealed class FakeSseEndpoint()
        : SseMinimalEndpoint<FakeSseRequest, FakeSsePayload>("test/sse")
    {
        protected override string? EventName => "todo-updated";

        protected override async IAsyncEnumerable<FakeSsePayload> Stream(
            FakeSseRequest request,
            HttpContext httpContext,
            [EnumeratorCancellation] CancellationToken ct)
        {
            yield return new FakeSsePayload("hello");
            await Task.CompletedTask;
        }
    }

    private sealed record FakeSseRequest;
    private sealed record FakeSsePayload(string Message);
}
