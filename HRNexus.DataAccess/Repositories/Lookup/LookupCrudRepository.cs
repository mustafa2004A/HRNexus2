using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Lookup;

public sealed class LookupCrudRepository<TEntity> : ILookupCrudRepository<TEntity>
    where TEntity : class
{
    private readonly HRNexusDbContext _dbContext;
    private readonly string _keyPropertyName;

    public LookupCrudRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
        _keyPropertyName = ResolveSingleIntegerKeyName();
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<int>(entity, _keyPropertyName) == id, cancellationToken);
    }

    public Task<TEntity?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(entity => EF.Property<int>(entity, _keyPropertyName) == id, cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    public void Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }

    private string ResolveSingleIntegerKeyName()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not configured.");

        var key = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have a configured primary key.");

        if (key.Properties.Count != 1 || key.Properties[0].ClrType != typeof(int))
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} must use a single integer primary key for lookup CRUD.");
        }

        return key.Properties[0].Name;
    }
}
