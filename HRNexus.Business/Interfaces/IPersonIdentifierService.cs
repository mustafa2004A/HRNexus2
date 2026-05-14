using HRNexus.Business.Models.Core;

namespace HRNexus.Business.Interfaces;

public interface IPersonIdentifierService
{
    Task<IReadOnlyList<PersonIdentifierDto>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default);
    Task<PersonIdentifierDto> GetByIdAsync(int personId, int identifierId, CancellationToken cancellationToken = default);
    Task<PersonIdentifierDto> CreateAsync(int personId, CreatePersonIdentifierRequest request, CancellationToken cancellationToken = default);
    Task<PersonIdentifierDto> UpdateAsync(int personId, int identifierId, UpdatePersonIdentifierRequest request, CancellationToken cancellationToken = default);
    Task<PersonIdentifierDto> DeleteAsync(int personId, int identifierId, CancellationToken cancellationToken = default);
}
