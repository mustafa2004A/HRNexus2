using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Core;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IPersonIdentifierRepository
{
    Task<IReadOnlyList<PersonIdentifierQueryResult>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default);
    Task<PersonIdentifierQueryResult?> GetByIdAsync(int personId, int identifierId, CancellationToken cancellationToken = default);
    Task<PersonIdentifier?> GetByIdForUpdateAsync(int personId, int identifierId, CancellationToken cancellationToken = default);
    Task AddAsync(PersonIdentifier identifier, CancellationToken cancellationToken = default);
    void Remove(PersonIdentifier identifier);
    Task DemotePrimaryIdentifiersAsync(int personId, int identifierTypeId, int? exceptIdentifierId = null, CancellationToken cancellationToken = default);
}
