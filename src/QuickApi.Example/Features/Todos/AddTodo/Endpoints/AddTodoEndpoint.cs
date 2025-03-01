using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Data.Contexts;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.AddTodo.Endpoints;


public record AddTodoRequest : IRequest<Todo>
{
    [FromBody] public AddTodoRequestBody Body { get; set; } = null!;
    public record AddTodoRequestBody(string Title, string? Description);
}

public class AddTodoEndpoint() : PostMinimalEndpoint<AddTodoRequest, Todo>("todos");

public class AddTodoRequestHandler : IRequestHandler<AddTodoRequest, Todo>
{
    private readonly IDbContext _context;

    public AddTodoRequestHandler(IDbContext context)
    {
        _context = context;
    }
    public async Task<Todo> Handle(AddTodoRequest request, CancellationToken cancellationToken)
    {
        var todo = Todo.Create(request.Body.Title, request.Body.Description);
        await _context.Set<Todo>().AddAsync(todo, cancellationToken);
        await _context.SaveChangeAsync(cancellationToken);
        return todo;
    }
}