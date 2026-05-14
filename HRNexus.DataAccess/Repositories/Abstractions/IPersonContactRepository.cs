using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Core;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IPersonContactRepository
{
    Task<IReadOnlyList<PersonContactQueryResult>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default);
    Task<PersonContactQueryResult?> GetByIdAsync(int personId, int contactId, CancellationToken cancellationToken = default);
    Task<PersonContact?> GetByIdForUpdateAsync(int personId, int contactId, CancellationToken cancellationToken = default);
    Task AddAsync(PersonContact contact, CancellationToken cancellationToken = default);
    void Remove(PersonContact contact);
    Task DemotePrimaryContactsAsync(int personId, int contactTypeId, int? exceptContactId = null, CancellationToken cancellationToken = default);
}
