using MediatR;
using QuickApi.Abstractions.Cqrs;

namespace QuickApi.Extensions.Mediatr.Cqrs;

public class MessageService(IMediator mediator) : IMessage
{
    public async Task<TResult> InvokeAsync<TResult>(object message, CancellationToken ct = default)
    {
        return await mediator.Send((IRequest<TResult>)message, ct);
    }

    public async Task InvokeAsync(object message, CancellationToken ct = default)
    {
        await mediator.Send(message, ct);
    }
}