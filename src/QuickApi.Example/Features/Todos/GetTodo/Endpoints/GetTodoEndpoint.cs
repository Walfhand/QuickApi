using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Data.Contexts;
using QuickApi.Example.Features.Todos.Domain;

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
    private readonly IDbContext _context;

    public GetTodoRequestHandler(IDbContext context)
    {
        _context = context;
    }
    public async Task<Todo> Handle(GetTodoRequest request, CancellationToken cancellationToken)
    {
        var todo = await _context.Set<Todo>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (todo is null)
            throw new Exception($"todo with {request.Id} Not found");
        return todo;
    }
}
