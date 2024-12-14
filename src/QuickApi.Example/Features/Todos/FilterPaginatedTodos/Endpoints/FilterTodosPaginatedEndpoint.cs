using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Engine.Web.Models;
using QuickApi.Example.Common.EfCore;
using QuickApi.Example.Data.Contexts;
using QuickApi.Example.Features.Todos.Domain;

namespace QuickApi.Example.Features.Todos.FilterPaginatedTodos.Endpoints;

public record FilterRequest(int PageIndex = 1, int PageSize = 20, string? SortBy = null, bool SortAscending = true);

public record FilterTodosPaginatedRequest : FilterRequest, IRequest<PaginatedResult<Todo>>
{
    public bool? IsCompleted { get; set; }
}



public class FilterTodosPaginatedEndpoint() : FilterPaginateMinimalEndpoint<FilterTodosPaginatedRequest, Todo>("todos/paginated")
{
    
}

public class FilterTodosPaginatedHandler : IRequestHandler<FilterTodosPaginatedRequest, PaginatedResult<Todo>>
{
    private readonly IDbContext _dbContext;

    public FilterTodosPaginatedHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PaginatedResult<Todo>> Handle(FilterTodosPaginatedRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<Todo>().AsQueryable();
        if (request.IsCompleted.HasValue)
            query = query.Where(x => x.IsCompleted == request.IsCompleted);
        return await query.ToPaginatedResultAsync(request.PageIndex, request.PageSize, request.SortBy,
            request.SortAscending, cancellationToken);
    }
}