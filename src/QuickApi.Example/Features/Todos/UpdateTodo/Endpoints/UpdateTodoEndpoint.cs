using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Data.Contexts;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.UpdateTodo.Endpoints;

public record UpdateTodoRequest : IRequest
{
    [FromRoute] public Guid Id { get; set; }
    [FromBody] public UpdateTodoRequestBody Body { get; set; } = null!;
    public record UpdateTodoRequestBody(string Title, string? Description);
}

public class UpdateTodoEndpoint() : PutMinimalEndpoint<UpdateTodoRequest>("todos/{id:guid}")
{
    
}

public class UpdateTodoEndpointHandler : IRequestHandler<UpdateTodoRequest>
{
    private readonly IDbContext _context;

    public UpdateTodoEndpointHandler(IDbContext context)
    {
        _context = context;
    }
    public async Task Handle(UpdateTodoRequest request, CancellationToken cancellationToken)
    {
        var todo = await _context.Set<Todo>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if(todo is null)
            throw new Exception($"todo with {request.Id} Not found");
        todo.Update(request.Body.Title, request.Body.Description);
        await _context.SaveChangeAsync(cancellationToken);
    }
}