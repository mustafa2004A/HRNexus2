using HRNexus.DataAccess.Entities.Leave;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IRequestStatusRepository
{
    Task<IReadOnlyList<RequestStatus>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<RequestStatus?> GetByIdAsync(int requestStatusId, CancellationToken cancellationToken = default);
    Task<RequestStatus?> GetByCodeAsync(string statusCode, CancellationToken cancellationToken = default);
}
