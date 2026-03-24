using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Data.Contexts;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.StreamTodos.Endpoints;

public record StreamTodosRequest
{
    [FromQuery] public bool? IsCompleted { get; set; }
    [FromQuery] public int PollIntervalSeconds { get; set; } = 3;
    [FromQuery] public bool IncludeHeartbeat { get; set; } = true;
}

public sealed record TodoStreamItem(Guid Id, string Title, string? Description, bool IsCompleted);

public sealed record StreamTodosEvent(long Sequence, string Name, int RetryMilliseconds, object Payload)
{
    public static StreamTodosEvent Snapshot(long sequence, int retryMilliseconds, IReadOnlyList<TodoStreamItem> items)
        => new(sequence, "todos-snapshot", retryMilliseconds, new SnapshotPayload(items, DateTimeOffset.UtcNow));

    public static StreamTodosEvent Heartbeat(long sequence, int retryMilliseconds, int totalCount)
        => new(sequence, "heartbeat", retryMilliseconds, new HeartbeatPayload(totalCount, DateTimeOffset.UtcNow));

    public sealed record SnapshotPayload(IReadOnlyList<TodoStreamItem> Items, DateTimeOffset SentAtUtc);
    public sealed record HeartbeatPayload(int TotalCount, DateTimeOffset SentAtUtc);
}

public class StreamTodosEndpoint : SseMinimalEndpoint<StreamTodosRequest, StreamTodosEvent>
{
    public StreamTodosEndpoint() : base("todos/stream")
    {
    }

    protected override async IAsyncEnumerable<StreamTodosEvent> Stream(
        StreamTodosRequest request,
        HttpContext httpContext,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var dbContext = httpContext.RequestServices.GetRequiredService<IDbContext>();
        var intervalSeconds = Math.Clamp(request.PollIntervalSeconds, 1, 30);
        var retryMilliseconds = intervalSeconds * 1000;
        var sequence = 0L;
        string? lastSnapshotSignature = null;

        while (!ct.IsCancellationRequested && !httpContext.RequestAborted.IsCancellationRequested)
        {
            var query = dbContext.Set<Todo>().AsNoTracking();
            if (request.IsCompleted.HasValue)
                query = query.Where(x => x.IsCompleted == request.IsCompleted.Value);

            var todos = await query.ToListAsync(ct);

            var items = todos
                .Select(x => new TodoStreamItem(x.Id.Value, x.Title, x.Description, x.IsCompleted))
                .OrderBy(x => x.Id)
                .ToList();

            var signature = BuildSignature(items);

            if (!string.Equals(signature, lastSnapshotSignature, StringComparison.Ordinal))
            {
                lastSnapshotSignature = signature;
                sequence++;
                yield return StreamTodosEvent.Snapshot(sequence, retryMilliseconds, items);
            }
            else if (request.IncludeHeartbeat)
            {
                sequence++;
                yield return StreamTodosEvent.Heartbeat(sequence, retryMilliseconds, items.Count);
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), ct);
        }
    }

    protected override async Task WriteEventAsync(HttpResponse response, StreamTodosEvent payload, CancellationToken ct)
    {
        await response.WriteAsync($"id: {payload.Sequence}\n", ct);
        await response.WriteAsync($"retry: {payload.RetryMilliseconds}\n", ct);
        await response.WriteAsync($"event: {payload.Name}\n", ct);

        var jsonPayload = JsonSerializer.Serialize(payload.Payload, SerializerOptions);
        foreach (var line in jsonPayload.Replace("\r\n", "\n").Split('\n'))
            await response.WriteAsync($"data: {line}\n", ct);

        await response.WriteAsync("\n", ct);
    }

    private static string BuildSignature(IReadOnlyCollection<TodoStreamItem> items)
        => string.Join('|', items.Select(x => $"{x.Id:N}:{x.Title}:{x.Description}:{x.IsCompleted}"));
}
