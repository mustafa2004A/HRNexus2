using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Core;

public sealed class AddressRepository : IAddressRepository
{
    private readonly HRNexusDbContext _dbContext;

    public AddressRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AddressQueryResult>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default)
    {
        return await CreateAddressQuery()
            .Where(address => address.PersonId == personId)
            .OrderByDescending(address => address.IsPrimary)
            .ThenBy(address => address.AddressType.Name)
            .ThenBy(address => address.AddressId)
            .Select(address => new AddressQueryResult(
                address.AddressId,
                address.PersonId,
                address.CityId,
                address.City.Name,
                address.AddressTypeId,
                address.AddressType.Name,
                address.AddressLine1,
                address.AddressLine2,
                address.Building,
                address.IsPrimary))
            .ToListAsync(cancellationToken);
    }

    public Task<AddressQueryResult?> GetByIdAsync(int personId, int addressId, CancellationToken cancellationToken = default)
    {
        return CreateAddressQuery()
            .Where(address => address.PersonId == personId && address.AddressId == addressId)
            .Select(address => new AddressQueryResult(
                address.AddressId,
                address.PersonId,
                address.CityId,
                address.City.Name,
                address.AddressTypeId,
                address.AddressType.Name,
                address.AddressLine1,
                address.AddressLine2,
                address.Building,
                address.IsPrimary))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Address?> GetByIdForUpdateAsync(int personId, int addressId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Addresses
            .FirstOrDefaultAsync(address => address.PersonId == personId && address.AddressId == addressId, cancellationToken);
    }

    public Task AddAsync(Address address, CancellationToken cancellationToken = default)
    {
        return _dbContext.Addresses.AddAsync(address, cancellationToken).AsTask();
    }

    public void Remove(Address address)
    {
        _dbContext.Addresses.Remove(address);
    }

    public async Task DemotePrimaryAddressesAsync(int personId, int? exceptAddressId = null, CancellationToken cancellationToken = default)
    {
        var addresses = await _dbContext.Addresses
            .Where(address =>
                address.PersonId == personId
                && address.IsPrimary
                && (!exceptAddressId.HasValue || address.AddressId != exceptAddressId.Value))
            .ToListAsync(cancellationToken);

        foreach (var address in addresses)
        {
            address.IsPrimary = false;
        }
    }

    private IQueryable<Address> CreateAddressQuery()
    {
        return _dbContext.Addresses.AsNoTracking();
    }

}
