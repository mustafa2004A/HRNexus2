using HRNexus.Business.Models.Files;
using HRNexus.Business.Models.Core;

namespace HRNexus.Business.Interfaces;

public interface IPersonService
{
    Task<IReadOnlyList<PersonDto>> ListAsync(string? search, bool includeDeleted, CancellationToken cancellationToken = default);
    Task<PersonDto> GetByIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<PersonDto> CreateAsync(CreatePersonRequest request, CancellationToken cancellationToken = default);
    Task<PersonDto> UpdateAsync(int personId, UpdatePersonRequest request, CancellationToken cancellationToken = default);
    Task<PersonPhotoDto> UploadPhotoAsync(int personId, FileUploadContent file, CancellationToken cancellationToken = default);
    Task<PersonDto> DeleteAsync(int personId, CancellationToken cancellationToken = default);
}
