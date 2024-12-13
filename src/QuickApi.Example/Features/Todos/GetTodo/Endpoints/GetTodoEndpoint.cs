using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Features.Domain;

namespace QuickApi.Example.Features.Todos.GetTodo.Endpoints;

public record GetTodoRequest : IRequest<Todo>
{
    [FromRoute] public Guid Id { get; set; }
}

public class GetTodoEndpoint() : GetMinimalEndpoint<GetTodoRequest, Todo>("todos/{id:guid}")
{
}

public class GetTodoRequestHandler : IRequestHandler<GetTodoRequest, Todo>
{
    public async Task<Todo> Handle(GetTodoRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        return Todo.Create("Test", "");
    }
}
