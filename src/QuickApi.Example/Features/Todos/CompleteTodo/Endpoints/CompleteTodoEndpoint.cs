using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Data.Contexts;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.CompleteTodo.Endpoints;

public record CompleteTodoRequest: IRequest
{
    [FromRoute] public Guid Id { get; set; }
} 

public class CompleteTodoEndpoint() : PatchMinimalEndpoint<CompleteTodoRequest>("todos/{id:guid}/complete")
{
    
}

public class CompleteTodoRequestHandler : IRequestHandler<CompleteTodoRequest>
{
    private readonly IDbContext _context;

    public CompleteTodoRequestHandler(IDbContext context)
    {
        _context = context;
    }
    public async Task Handle(CompleteTodoRequest request, CancellationToken cancellationToken)
    {
        var todo = await _context.Set<Todo>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (todo is null)
            throw new Exception($"todo with {request.Id} Not found");
        todo.Complete();
        await _context.SaveChangeAsync(cancellationToken);
    }
}