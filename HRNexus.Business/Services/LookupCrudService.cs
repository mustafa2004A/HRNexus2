using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Lookup;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.Business.Services;

public sealed class LookupCrudService<TEntity, TDto, TCreateRequest, TUpdateRequest> : ILookupCrudService<TDto, TCreateRequest, TUpdateRequest>
    where TEntity : class
{
    private readonly ILookupCrudRepository<TEntity> _repository;
    private readonly ILookupCrudDefinition<TEntity, TDto, TCreateRequest, TUpdateRequest> _definition;
    private readonly IHRNexusDbContext _dbContext;

    public LookupCrudService(
        ILookupCrudRepository<TEntity> repository,
        ILookupCrudDefinition<TEntity, TDto, TCreateRequest, TUpdateRequest> definition,
        IHRNexusDbContext dbContext)
    {
        _repository = repository;
        _definition = definition;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.ListAsync(cancellationToken);

        return entities
            .OrderBy(_definition.GetSortText, StringComparer.OrdinalIgnoreCase)
            .Select(_definition.ToDto)
            .ToList();
    }

    public async Task<TDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw CreateNotFoundException(id);

        return _definition.ToDto(entity);
    }

    public async Task<LookupCrudResult<TDto>> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _definition.ValidateCreate(request);
        var entity = _definition.CreateEntity(request);

        await _repository.AddAsync(entity, cancellationToken);
        await SaveChangesAsync("create", cancellationToken);

        return new LookupCrudResult<TDto>(_definition.GetId(entity), _definition.ToDto(entity));
    }

    public async Task<TDto> UpdateAsync(int id, TUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw CreateNotFoundException(id);

        _definition.ValidateUpdate(id, request);
        _definition.UpdateEntity(entity, request);

        await SaveChangesAsync("update", cancellationToken);
        return _definition.ToDto(entity);
    }

    public async Task<TDto> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw CreateNotFoundException(id);

        var deletedDto = _definition.ToDto(entity);

        if (_definition.UsesSoftDelete)
        {
            _definition.Deactivate(entity);
            await SaveChangesAsync("deactivate", cancellationToken);
            return _definition.ToDto(entity);
        }

        _repository.Remove(entity);
        await SaveChangesAsync("delete", cancellationToken);
        return deletedDto;
    }

    private EntityNotFoundException CreateNotFoundException(int id)
    {
        return new EntityNotFoundException($"{_definition.EntityName} {id} was not found.");
    }

    private async Task SaveChangesAsync(string operationName, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new BusinessRuleException(
                $"Unable to {operationName} {_definition.EntityName}. Check for duplicate values or records already in use.",
                exception);
        }
    }
}
