namespace QuickApi.Engine.Web.Models;

public record PaginatedResult<T>(IEnumerable<T> Items, int TotalCount, int PageIndex, int PageSize);