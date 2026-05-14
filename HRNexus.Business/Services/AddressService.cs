using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Core;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class AddressService : IAddressService
{
    private readonly IPersonRepository _personRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IHRNexusDbContext _dbContext;

    public AddressService(
        IPersonRepository personRepository,
        IAddressRepository addressRepository,
        IReferenceDataRepository referenceDataRepository,
        IHRNexusDbContext dbContext)
    {
        _personRepository = personRepository;
        _addressRepository = addressRepository;
        _referenceDataRepository = referenceDataRepository;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AddressDto>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var addresses = await _addressRepository.GetByPersonAsync(personId, cancellationToken);
        return addresses.Select(OperationalServiceHelpers.ToAddressDto).ToList();
    }

    public async Task<AddressDto> GetByIdAsync(int personId, int addressId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var address = await _addressRepository.GetByIdAsync(personId, addressId, cancellationToken)
            ?? throw AddressNotFound(addressId);

        return OperationalServiceHelpers.ToAddressDto(address);
    }

    public async Task<AddressDto> CreateAsync(int personId, CreateAddressRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateAsync(personId, request.CityId, request.AddressTypeId, cancellationToken);

        if (request.IsPrimary)
        {
            await _addressRepository.DemotePrimaryAddressesAsync(personId, cancellationToken: cancellationToken);
        }

        var address = new Address
        {
            PersonId = personId,
            CityId = request.CityId,
            AddressTypeId = request.AddressTypeId,
            AddressLine1 = OperationalServiceHelpers.RequiredText(request.AddressLine1, "Address line 1"),
            AddressLine2 = OperationalServiceHelpers.OptionalText(request.AddressLine2),
            Building = OperationalServiceHelpers.OptionalText(request.Building),
            IsPrimary = request.IsPrimary
        };

        await _addressRepository.AddAsync(address, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create address", cancellationToken);
        return await GetByIdAsync(personId, address.AddressId, cancellationToken);
    }

    public async Task<AddressDto> UpdateAsync(int personId, int addressId, UpdateAddressRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateAsync(personId, request.CityId, request.AddressTypeId, cancellationToken);

        var address = await _addressRepository.GetByIdForUpdateAsync(personId, addressId, cancellationToken)
            ?? throw AddressNotFound(addressId);

        if (request.IsPrimary)
        {
            await _addressRepository.DemotePrimaryAddressesAsync(personId, addressId, cancellationToken);
        }

        address.CityId = request.CityId;
        address.AddressTypeId = request.AddressTypeId;
        address.AddressLine1 = OperationalServiceHelpers.RequiredText(request.AddressLine1, "Address line 1");
        address.AddressLine2 = OperationalServiceHelpers.OptionalText(request.AddressLine2);
        address.Building = OperationalServiceHelpers.OptionalText(request.Building);
        address.IsPrimary = request.IsPrimary;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update address", cancellationToken);
        return await GetByIdAsync(personId, addressId, cancellationToken);
    }

    public async Task<AddressDto> DeleteAsync(int personId, int addressId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var existing = await GetByIdAsync(personId, addressId, cancellationToken);
        var address = await _addressRepository.GetByIdForUpdateAsync(personId, addressId, cancellationToken)
            ?? throw AddressNotFound(addressId);

        _addressRepository.Remove(address);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "delete address", cancellationToken);
        return existing;
    }

    private async Task ValidateAsync(int personId, int cityId, int addressTypeId, CancellationToken cancellationToken)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);

        if (!await _referenceDataRepository.CityExistsAsync(cityId, cancellationToken))
        {
            throw new BusinessRuleException($"City {cityId} was not found.");
        }

        if (!await _referenceDataRepository.AddressTypeExistsAsync(addressTypeId, cancellationToken))
        {
            throw new BusinessRuleException($"Address type {addressTypeId} was not found.");
        }
    }

    private async Task EnsurePersonExistsAsync(int personId, CancellationToken cancellationToken)
    {
        if (!await _personRepository.ExistsAsync(personId, cancellationToken: cancellationToken))
        {
            throw new EntityNotFoundException($"Person {personId} was not found.");
        }
    }

    private static EntityNotFoundException AddressNotFound(int addressId)
    {
        return new EntityNotFoundException($"Address {addressId} was not found.");
    }
}
