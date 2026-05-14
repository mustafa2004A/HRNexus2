using HRNexus.DataAccess.Entities.Core;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IFileStorageRepository
{
    Task<FileStorageItem?> GetByIdAsync(int fileStorageItemId, CancellationToken cancellationToken = default);
    Task<FileStorageItem?> GetByIdForUpdateAsync(int fileStorageItemId, CancellationToken cancellationToken = default);
    Task<FileStorageItem?> GetActiveDuplicateAsync(string fileCategory, string fileHashSha256, long fileSizeBytes, CancellationToken cancellationToken = default);
    Task AddAsync(FileStorageItem fileStorageItem, CancellationToken cancellationToken = default);
}
