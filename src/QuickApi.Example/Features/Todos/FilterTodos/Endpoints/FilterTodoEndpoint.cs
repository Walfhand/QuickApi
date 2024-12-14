using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Example.Data.Contexts;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.FilterTodos.Endpoints;

public record FilterTodoRequest : IRequest<List<Todo>>
{
    public bool? IsCompleted { get; set; }
}
public class FilterTodoEndpoint() : FilterMinimalEndpoint<FilterTodoRequest, Todo>("todos")
{
    
}

public class FilterTodoEndpointCustom() : FilterMinimalEndpoint<FilterTodoRequest, Todo>("todos/custom")
{
    protected override void Configure(RouteHandlerBuilder routeBuilder)
    {
        base.Configure(routeBuilder);
        routeBuilder.ProducesProblem(500);
    }

    protected override Delegate Handler => EndpointHandler;

    private static async Task<IResult> EndpointHandler([AsParameters] FilterTodoRequest request, IDbContext context, CancellationToken ct)
    {
        var query = context.Set<Todo>().AsQueryable();
        if(request.IsCompleted.HasValue)
            query = query.Where(x => x.IsCompleted == request.IsCompleted);
        return Results.Ok(await query.ToListAsync(ct));
    }
}

public class FilterTodoRequestHandler : IRequestHandler<FilterTodoRequest, List<Todo>>
{
    private readonly IDbContext _context;

    public FilterTodoRequestHandler(IDbContext context)
    {
        _context = context;
    }
    public async Task<List<Todo>> Handle(FilterTodoRequest request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Todo>().AsQueryable();
        if(request.IsCompleted.HasValue)
            query = query.Where(x => x.IsCompleted == request.IsCompleted);
        return await query.ToListAsync(cancellationToken);
    }
}