using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Core;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IAddressRepository
{
    Task<IReadOnlyList<AddressQueryResult>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default);
    Task<AddressQueryResult?> GetByIdAsync(int personId, int addressId, CancellationToken cancellationToken = default);
    Task<Address?> GetByIdForUpdateAsync(int personId, int addressId, CancellationToken cancellationToken = default);
    Task AddAsync(Address address, CancellationToken cancellationToken = default);
    void Remove(Address address);
    Task DemotePrimaryAddressesAsync(int personId, int? exceptAddressId = null, CancellationToken cancellationToken = default);
}
