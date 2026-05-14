using HRNexus.Business.Models.Core;

namespace HRNexus.Business.Interfaces;

public interface IPersonContactService
{
    Task<IReadOnlyList<PersonContactDto>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default);
    Task<PersonContactDto> GetByIdAsync(int personId, int contactId, CancellationToken cancellationToken = default);
    Task<PersonContactDto> CreateAsync(int personId, CreatePersonContactRequest request, CancellationToken cancellationToken = default);
    Task<PersonContactDto> UpdateAsync(int personId, int contactId, UpdatePersonContactRequest request, CancellationToken cancellationToken = default);
    Task<PersonContactDto> DeleteAsync(int personId, int contactId, CancellationToken cancellationToken = default);
}
