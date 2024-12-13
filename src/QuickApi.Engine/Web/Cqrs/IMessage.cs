namespace QuickApi.Engine.Web.Cqrs;

public interface IMessage
{
    Task<TResult> InvokeAsync<TResult>(object message, CancellationToken ct = default);
    Task InvokeAsync(object message, CancellationToken ct = default);
}