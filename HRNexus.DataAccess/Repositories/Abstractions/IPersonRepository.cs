using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Core;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IPersonRepository
{
    Task<IReadOnlyList<PersonQueryResult>> ListAsync(string? search, bool includeDeleted, CancellationToken cancellationToken = default);
    Task<PersonQueryResult?> GetByIdAsync(int personId, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<Person?> GetByIdForUpdateAsync(int personId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int personId, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task AddAsync(Person person, CancellationToken cancellationToken = default);
}
