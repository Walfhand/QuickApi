using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using QuickApi.Engine.Web.Endpoints.Enums;

namespace QuickApi.Engine.Web.Endpoints.Impl;

public abstract class SseMinimalEndpoint<TRequest, TEvent>(
    [StringSyntax("Route")] string path,
    params string[] policies)
    : MinimalEndpoint<TEvent>(EndpointType.Sse, path, policies)
    where TRequest : class
{
    protected virtual string? EventName => null;
    protected virtual JsonSerializerOptions SerializerOptions => new(JsonSerializerDefaults.Web);

    protected override Delegate Handler => Endpoint;

    protected abstract IAsyncEnumerable<TEvent> Stream(
        TRequest request,
        HttpContext httpContext,
        CancellationToken ct);

    private async Task Endpoint([AsParameters] TRequest request, HttpContext httpContext, CancellationToken ct)
    {
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.Headers.CacheControl = "no-cache";
        httpContext.Response.Headers.Connection = "keep-alive";
        httpContext.Response.Headers["X-Accel-Buffering"] = "no";
        httpContext.Response.ContentType = "text/event-stream";

        try
        {
            await foreach (var message in Stream(request, httpContext, ct).WithCancellation(ct))
            {
                if (httpContext.RequestAborted.IsCancellationRequested)
                    break;

                await WriteEventAsync(httpContext.Response, message, ct);
                await httpContext.Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested || httpContext.RequestAborted.IsCancellationRequested)
        {
            // Ignore canceled requests to avoid noisy logs on disconnected clients.
        }
    }

    protected virtual async Task WriteEventAsync(HttpResponse response, TEvent payload, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(EventName))
            await response.WriteAsync($"event: {EventName}\n", ct);

        var message = payload switch
        {
            null => "null",
            string text => text,
            _ => JsonSerializer.Serialize(payload, SerializerOptions)
        };

        var lines = message.Replace("\r\n", "\n").Split('\n');
        foreach (var line in lines)
            await response.WriteAsync($"data: {line}\n", ct);

        await response.WriteAsync("\n", ct);
    }
}
