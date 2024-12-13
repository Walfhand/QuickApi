using MediatR;
using QuickApi.Engine.Web.Endpoints.Impl;
using QuickApi.Engine.Web.Models;
using QuickApi.Example.Features.Domain;

namespace QuickApi.Example.Features.Todos.FilterPaginatedTodos.Endpoints;

public record FilterRequest(int PageIndex = 1, int PageSize = 20, string? SortBy = null, bool SortAscending = true);

public record FilterTodosPaginatedRequest : FilterRequest, IRequest<PaginatedResult<Todo>>
{
    public bool? IsCompleted { get; set; }
}



public class FilterTodosPaginatedEndpoint() : FilterPaginateMinimalEndpoint<FilterTodosPaginatedRequest, Todo>("todos")
{
    
}

public class FilterTodosPaginatedHandler : IRequestHandler<FilterTodosPaginatedRequest, PaginatedResult<Todo>>
{
    public async Task<PaginatedResult<Todo>> Handle(FilterTodosPaginatedRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        var todos = new List<Todo>{ Todo.Create("title", "") };
        return new PaginatedResult<Todo>(todos, todos.Count, 1, todos.Count);
    }
}