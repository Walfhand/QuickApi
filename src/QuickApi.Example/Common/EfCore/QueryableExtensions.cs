using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Models;

namespace QuickApi.Example.Common.EfCore;

public static class QueryableExtensions
{
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(sortBy)) query = query.OrderByDynamic(sortBy, ascending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, totalCount, pageIndex, pageSize);
    }
    
    
    private static IQueryable<T> OrderByDynamic<T>(
        this IQueryable<T> query,
        string sortBy,
        bool ascending)
    {
        var param = Expression.Parameter(typeof(T), "x");

        var property = sortBy.Split('.').Aggregate<string?, Expression>(param, Expression.Property!);

        var lambda = Expression.Lambda(property, param);

        var methodName = ascending ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        return (IQueryable<T>)method.Invoke(null, [query, lambda])!;
    }
}