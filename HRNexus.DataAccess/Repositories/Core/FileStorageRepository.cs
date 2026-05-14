using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Core;

public sealed class FileStorageRepository : IFileStorageRepository
{
    private readonly HRNexusDbContext _dbContext;

    public FileStorageRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FileStorageItem?> GetByIdAsync(int fileStorageItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FileStorageItems
            .AsNoTracking()
            .FirstOrDefaultAsync(file => file.FileStorageItemId == fileStorageItemId, cancellationToken);
    }

    public Task<FileStorageItem?> GetByIdForUpdateAsync(int fileStorageItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FileStorageItems
            .FirstOrDefaultAsync(file => file.FileStorageItemId == fileStorageItemId, cancellationToken);
    }

    public Task<FileStorageItem?> GetActiveDuplicateAsync(
        string fileCategory,
        string fileHashSha256,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.FileStorageItems
            .AsNoTracking()
            .FirstOrDefaultAsync(file =>
                file.IsActive
                && file.FileCategory == fileCategory
                && file.FileHashSha256 == fileHashSha256
                && file.FileSizeBytes == fileSizeBytes,
                cancellationToken);
    }

    public Task AddAsync(FileStorageItem fileStorageItem, CancellationToken cancellationToken = default)
    {
        return _dbContext.FileStorageItems.AddAsync(fileStorageItem, cancellationToken).AsTask();
    }
}
