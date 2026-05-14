using HRNexus.Business.Models.Lookup;

namespace HRNexus.Business.Interfaces;

public interface ILookupCrudService<TDto, TCreateRequest, TUpdateRequest>
{
    Task<IReadOnlyList<TDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<TDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LookupCrudResult<TDto>> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default);
    Task<TDto> UpdateAsync(int id, TUpdateRequest request, CancellationToken cancellationToken = default);
    Task<TDto> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
