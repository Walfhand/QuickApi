using MediatR;
using QuickApi.Engine.Web.Cqrs;

namespace QuickApi.Example.Cqrs;

public class MessageService : IMessage
{
    private readonly IMediator _mediator;

    public MessageService(IMediator mediator)
    {
        _mediator = mediator;
    }
    public async Task<TResult> InvokeAsync<TResult>(object message, CancellationToken ct = default)
    {
        return await _mediator.Send((IRequest<TResult>)message, ct);
    }

    public async Task InvokeAsync(object message, CancellationToken ct = default)
    {
        await _mediator.Send(message, ct);
    }
}