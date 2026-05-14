using HRNexus.Business.Models.Files;

namespace HRNexus.Business.Interfaces;

public interface IFileStorageService
{
    Task<FileStorageItemDto> SaveAsync(
        string fileCategory,
        FileUploadContent upload,
        int? uploadedByUserId,
        CancellationToken cancellationToken = default);

    Task<FileIntegrityVerificationResultDto> VerifyIntegrityAsync(
        int fileStorageItemId,
        CancellationToken cancellationToken = default);
}
