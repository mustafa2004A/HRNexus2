using System.Security.Cryptography;
using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Files;
using HRNexus.Business.Options;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.Extensions.Options;

namespace HRNexus.Business.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private const string HashAlgorithm = "SHA-256";

    private readonly IFileStorageRepository _fileStorageRepository;
    private readonly IHRNexusDbContext _dbContext;
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(
        IFileStorageRepository fileStorageRepository,
        IHRNexusDbContext dbContext,
        IOptions<FileStorageOptions> options)
    {
        _fileStorageRepository = fileStorageRepository;
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<FileStorageItemDto> SaveAsync(
        string fileCategory,
        FileUploadContent upload,
        int? uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(upload);
        var category = NormalizeCategory(fileCategory);
        var originalFileName = NormalizeOriginalFileName(upload.OriginalFileName);
        var extension = NormalizeExtension(originalFileName);
        EnsureAllowedExtension(category, extension);
        EnsureValidLength(upload.Length);

        var categoryFolder = GetCategoryFolder(category);
        var targetDirectory = GetSafeStorageDirectory(categoryFolder);
        Directory.CreateDirectory(targetDirectory);

        var tempPath = Path.Combine(targetDirectory, $"{Guid.NewGuid():N}.tmp");

        try
        {
            var stagedFile = await StageAndHashAsync(upload.Content, tempPath, cancellationToken);
            EnsureValidLength(stagedFile.FileSizeBytes);

            var duplicate = await _fileStorageRepository.GetActiveDuplicateAsync(
                category,
                stagedFile.Hash,
                stagedFile.FileSizeBytes,
                cancellationToken);

            if (duplicate is not null)
            {
                DeleteIfExists(tempPath);
                return MapFile(duplicate);
            }

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var finalPath = Path.Combine(targetDirectory, storedFileName);
            File.Move(tempPath, finalPath);

            var relativePath = CreateRelativePath(categoryFolder, storedFileName);
            var fileStorageItem = new FileStorageItem
            {
                FileCategory = category,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                RelativePath = relativePath,
                ContentType = NormalizeContentType(upload.ContentType),
                FileExtension = extension,
                FileSizeBytes = stagedFile.FileSizeBytes,
                FileHashSha256 = stagedFile.Hash,
                HashAlgorithm = HashAlgorithm,
                UploadedByUserId = uploadedByUserId,
                UploadedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _fileStorageRepository.AddAsync(fileStorageItem, cancellationToken);

            try
            {
                await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "save file storage metadata", cancellationToken);
            }
            catch
            {
                DeleteIfExists(finalPath);
                throw;
            }

            return MapFile(fileStorageItem);
        }
        finally
        {
            DeleteIfExists(tempPath);
        }
    }

    public async Task<FileIntegrityVerificationResultDto> VerifyIntegrityAsync(
        int fileStorageItemId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileStorageRepository.GetByIdForUpdateAsync(fileStorageItemId, cancellationToken)
            ?? throw new EntityNotFoundException($"File storage item {fileStorageItemId} was not found.");

        var checkedAt = DateTime.UtcNow;
        var physicalPath = GetSafePhysicalPath(file.RelativePath);
        var fileExists = File.Exists(physicalPath);
        var isHashValid = false;
        var isSizeValid = false;

        if (fileExists)
        {
            var fileInfo = new FileInfo(physicalPath);
            isSizeValid = fileInfo.Length == file.FileSizeBytes;

            await using var fileStream = fileInfo.OpenRead();
            var hash = await ComputeSha256Async(fileStream, cancellationToken);
            isHashValid = string.Equals(hash, file.FileHashSha256, StringComparison.OrdinalIgnoreCase);
        }

        file.LastIntegrityCheckAt = checkedAt;
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update file integrity check timestamp", cancellationToken);

        return new FileIntegrityVerificationResultDto(
            file.FileStorageItemId,
            file.FileCategory,
            file.RelativePath,
            fileExists,
            file.HashAlgorithm,
            isHashValid,
            isSizeValid,
            fileExists && isHashValid && isSizeValid,
            checkedAt);
    }

    private static async Task<StagedFileResult> StageAndHashAsync(
        Stream source,
        string tempPath,
        CancellationToken cancellationToken)
    {
        await using var target = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        var buffer = new byte[81920];
        long totalBytes = 0;

        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            await target.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            sha256.AppendData(buffer.AsSpan(0, bytesRead));
            totalBytes += bytesRead;
        }

        var hash = Convert.ToHexString(sha256.GetHashAndReset()).ToLowerInvariant();

        return new StagedFileResult(totalBytes, hash);
    }

    private static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static FileStorageItemDto MapFile(FileStorageItem file)
    {
        return new FileStorageItemDto(
            file.FileStorageItemId,
            file.FileCategory,
            file.OriginalFileName,
            file.StoredFileName,
            file.RelativePath,
            file.ContentType,
            file.FileExtension,
            file.FileSizeBytes,
            file.FileHashSha256,
            file.HashAlgorithm,
            file.UploadedByUserId,
            file.UploadedAt,
            file.IsActive,
            file.LastIntegrityCheckAt);
    }

    private static string NormalizeCategory(string fileCategory)
    {
        if (string.IsNullOrWhiteSpace(fileCategory))
        {
            throw new BusinessRuleException("File category is required.");
        }

        return fileCategory.Trim();
    }

    private static string NormalizeOriginalFileName(string originalFileName)
    {
        var fileName = Path.GetFileName(originalFileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BusinessRuleException("Uploaded file name is required.");
        }

        return fileName.Trim();
    }

    private static string NormalizeExtension(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new BusinessRuleException("Uploaded file must include a file extension.");
        }

        return extension.Trim().ToLowerInvariant();
    }

    private static string? NormalizeContentType(string? contentType)
    {
        return string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim();
    }

    private void EnsureAllowedExtension(string category, string extension)
    {
        var allowedExtensions = category switch
        {
            FileStorageCategories.LeaveAttachment => _options.AllowedAttachmentExtensions,
            FileStorageCategories.EmployeeDocument => _options.AllowedAttachmentExtensions,
            FileStorageCategories.PersonPhoto => _options.AllowedPhotoExtensions,
            _ => throw new BusinessRuleException($"Unsupported file category '{category}'.")
        };

        if (!allowedExtensions.Any(allowed => string.Equals(allowed, extension, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessRuleException($"File extension '{extension}' is not allowed for {category} files.");
        }
    }

    private void EnsureValidLength(long length)
    {
        if (length <= 0)
        {
            throw new BusinessRuleException("Uploaded file cannot be empty.");
        }

        if (length > _options.MaxFileSizeBytes)
        {
            throw new BusinessRuleException($"Uploaded file exceeds the maximum allowed size of {_options.MaxFileSizeBytes} bytes.");
        }
    }

    private string GetCategoryFolder(string category)
    {
        return category switch
        {
            FileStorageCategories.LeaveAttachment => _options.LeaveAttachmentsFolder,
            FileStorageCategories.EmployeeDocument => _options.EmployeeDocumentsFolder,
            FileStorageCategories.PersonPhoto => _options.PersonPhotosFolder,
            _ => throw new BusinessRuleException($"Unsupported file category '{category}'.")
        };
    }

    private string GetSafeStorageDirectory(string categoryFolder)
    {
        var root = GetSafeRootPath();
        var folderName = Path.GetFileName(categoryFolder.Trim());
        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new BusinessRuleException("File storage folder is not configured.");
        }

        var directory = Path.GetFullPath(Path.Combine(root, folderName));
        EnsureWithinRoot(root, directory);
        return directory;
    }

    private string GetSafePhysicalPath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new BusinessRuleException("Stored relative path is invalid.");
        }

        var root = GetSafeRootPath();
        var safeRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.GetFullPath(Path.Combine(root, safeRelativePath));
        EnsureWithinRoot(root, physicalPath);
        return physicalPath;
    }

    private string GetSafeRootPath()
    {
        if (string.IsNullOrWhiteSpace(_options.RootPath))
        {
            throw new BusinessRuleException("File storage root path is not configured.");
        }

        return Path.GetFullPath(_options.RootPath.Trim());
    }

    private static void EnsureWithinRoot(string root, string path)
    {
        var normalizedRoot = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Resolved file path is outside the configured storage root.");
        }
    }

    private static string CreateRelativePath(string categoryFolder, string storedFileName)
    {
        var folderName = Path.GetFileName(categoryFolder.Trim());
        return $"{folderName}/{storedFileName}";
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed record StagedFileResult(long FileSizeBytes, string Hash);
}
