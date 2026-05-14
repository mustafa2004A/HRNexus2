using HRNexus.Business.Models.Core;

namespace HRNexus.Business.Interfaces;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default);
    Task<AddressDto> GetByIdAsync(int personId, int addressId, CancellationToken cancellationToken = default);
    Task<AddressDto> CreateAsync(int personId, CreateAddressRequest request, CancellationToken cancellationToken = default);
    Task<AddressDto> UpdateAsync(int personId, int addressId, UpdateAddressRequest request, CancellationToken cancellationToken = default);
    Task<AddressDto> DeleteAsync(int personId, int addressId, CancellationToken cancellationToken = default);
}
