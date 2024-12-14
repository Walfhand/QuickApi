using Microsoft.EntityFrameworkCore;

namespace QuickApi.Example.Data.Contexts;

public interface IDbContext
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;
    Task<int> SaveChangeAsync(CancellationToken cancellationToken = default);
}