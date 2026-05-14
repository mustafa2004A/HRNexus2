using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Core;

public sealed class PersonIdentifierRepository : IPersonIdentifierRepository
{
    private readonly HRNexusDbContext _dbContext;

    public PersonIdentifierRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PersonIdentifierQueryResult>> GetByPersonAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        return await CreateIdentifierQuery()
            .Where(identifier => identifier.PersonId == personId)
            .OrderByDescending(identifier => identifier.IsPrimary)
            .ThenBy(identifier => identifier.IdentifierType.Name)
            .ThenBy(identifier => identifier.PersonIdentifierId)
            .Select(identifier => new PersonIdentifierQueryResult(
                identifier.PersonIdentifierId,
                identifier.PersonId,
                identifier.IdentifierTypeId,
                identifier.IdentifierType.Name,
                identifier.IdentifierValue,
                identifier.IsPrimary,
                identifier.IssueDate,
                identifier.ExpiryDate,
                identifier.CountryId,
                identifier.Country == null ? null : identifier.Country.Name,
                identifier.CreatedDate))
            .ToListAsync(cancellationToken);
    }

    public Task<PersonIdentifierQueryResult?> GetByIdAsync(
        int personId,
        int identifierId,
        CancellationToken cancellationToken = default)
    {
        return CreateIdentifierQuery()
            .Where(identifier => identifier.PersonId == personId && identifier.PersonIdentifierId == identifierId)
            .Select(identifier => new PersonIdentifierQueryResult(
                identifier.PersonIdentifierId,
                identifier.PersonId,
                identifier.IdentifierTypeId,
                identifier.IdentifierType.Name,
                identifier.IdentifierValue,
                identifier.IsPrimary,
                identifier.IssueDate,
                identifier.ExpiryDate,
                identifier.CountryId,
                identifier.Country == null ? null : identifier.Country.Name,
                identifier.CreatedDate))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<PersonIdentifier?> GetByIdForUpdateAsync(
        int personId,
        int identifierId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.PersonIdentifiers
            .FirstOrDefaultAsync(identifier => identifier.PersonId == personId && identifier.PersonIdentifierId == identifierId, cancellationToken);
    }

    public Task AddAsync(PersonIdentifier identifier, CancellationToken cancellationToken = default)
    {
        return _dbContext.PersonIdentifiers.AddAsync(identifier, cancellationToken).AsTask();
    }

    public void Remove(PersonIdentifier identifier)
    {
        _dbContext.PersonIdentifiers.Remove(identifier);
    }

    public async Task DemotePrimaryIdentifiersAsync(
        int personId,
        int identifierTypeId,
        int? exceptIdentifierId = null,
        CancellationToken cancellationToken = default)
    {
        var identifiers = await _dbContext.PersonIdentifiers
            .Where(identifier =>
                identifier.PersonId == personId
                && identifier.IdentifierTypeId == identifierTypeId
                && identifier.IsPrimary
                && (!exceptIdentifierId.HasValue || identifier.PersonIdentifierId != exceptIdentifierId.Value))
            .ToListAsync(cancellationToken);

        foreach (var identifier in identifiers)
        {
            identifier.IsPrimary = false;
        }
    }

    private IQueryable<PersonIdentifier> CreateIdentifierQuery()
    {
        return _dbContext.PersonIdentifiers.AsNoTracking();
    }

}
