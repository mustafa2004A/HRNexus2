using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Files;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class LeaveAttachmentService : ILeaveAttachmentService
{
    private static readonly string[] AttachmentAccessRoles = ["Admin", "HRManager", "HRClerk", "DepartmentManager"];

    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILeaveAttachmentRepository _leaveAttachmentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;

    public LeaveAttachmentService(
        ILeaveRequestRepository leaveRequestRepository,
        IUserRepository userRepository,
        ILeaveAttachmentRepository leaveAttachmentRepository,
        IFileStorageService fileStorageService,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _leaveAttachmentRepository = leaveAttachmentRepository;
        _fileStorageService = fileStorageService;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
    }

    public async Task<LeaveAttachmentDto> UploadAttachmentAsync(
        int leaveRequestId,
        FileUploadContent file,
        int? uploadedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (leaveRequestId <= 0)
        {
            throw new BusinessRuleException("Leave request id must be a positive number.");
        }

        ArgumentNullException.ThrowIfNull(file);
        await EnsureCanAccessLeaveRequestAsync(leaveRequestId, cancellationToken);

        var effectiveUploadedByUserId = uploadedByUserId ?? _currentUserContext.UserId
            ?? throw new BusinessRuleException("Uploaded by user is required.");

        var uploader = await _userRepository.GetByIdAsync(effectiveUploadedByUserId, cancellationToken)
            ?? throw new EntityNotFoundException($"User {effectiveUploadedByUserId} was not found.");

        var storedFile = await _fileStorageService.SaveAsync(
            FileStorageCategories.LeaveAttachment,
            file,
            effectiveUploadedByUserId,
            cancellationToken);

        var attachment = new LeaveAttachment
        {
            LeaveRequestId = leaveRequestId,
            FileStorageItemId = storedFile.FileStorageItemId,
            FileName = storedFile.OriginalFileName,
            FilePath = storedFile.RelativePath,
            FileExtension = storedFile.FileExtension,
            UploadedBy = effectiveUploadedByUserId,
            UploadedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _leaveAttachmentRepository.AddAsync(attachment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LeaveAttachmentDto(
            attachment.LeaveAttachmentId,
            attachment.LeaveRequestId,
            attachment.FileName,
            OperationalServiceHelpers.ToSafeRelativeFilePath(attachment.FilePath),
            attachment.FileExtension,
            attachment.FileStorageItemId,
            attachment.UploadedBy,
            uploader.Username,
            attachment.UploadedAt,
            attachment.IsActive);
    }

    public async Task<IReadOnlyList<LeaveAttachmentDto>> GetLeaveRequestAttachmentsAsync(int leaveRequestId, CancellationToken cancellationToken = default)
    {
        await EnsureCanAccessLeaveRequestAsync(leaveRequestId, cancellationToken);

        var attachments = await _leaveAttachmentRepository.GetByLeaveRequestAsync(leaveRequestId, cancellationToken);

        return attachments
            .Select(MapAttachment)
            .ToList();
    }

    public async Task<LeaveAttachmentDto> GetAttachmentAsync(int leaveAttachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _leaveAttachmentRepository.GetByIdAsync(leaveAttachmentId, cancellationToken)
            ?? throw new EntityNotFoundException($"Leave attachment {leaveAttachmentId} was not found.");

        await EnsureCanAccessLeaveRequestAsync(attachment.LeaveRequestId, cancellationToken);

        return MapAttachment(attachment);
    }

    public async Task<LeaveAttachmentDto> DeactivateAttachmentAsync(int leaveAttachmentId, CancellationToken cancellationToken = default)
    {
        var attachment = await _leaveAttachmentRepository.GetByIdForUpdateAsync(leaveAttachmentId, cancellationToken)
            ?? throw new EntityNotFoundException($"Leave attachment {leaveAttachmentId} was not found.");

        await EnsureCanAccessLeaveRequestAsync(attachment.LeaveRequestId, cancellationToken);

        attachment.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetAttachmentAsync(leaveAttachmentId, cancellationToken);
    }

    private async Task EnsureCanAccessLeaveRequestAsync(int leaveRequestId, CancellationToken cancellationToken)
    {
        var ownerEmployeeId = await _leaveRequestRepository.GetEmployeeIdAsync(leaveRequestId, cancellationToken);

        if (!ownerEmployeeId.HasValue)
        {
            throw new EntityNotFoundException($"Leave request {leaveRequestId} was not found.");
        }

        if (!_currentUserContext.UserId.HasValue)
        {
            throw new AuthorizationFailedException("Authentication is required to access leave attachments.");
        }

        if (IsInAnyRole(AttachmentAccessRoles)
            || (_currentUserContext.EmployeeId.HasValue && _currentUserContext.EmployeeId.Value == ownerEmployeeId.Value))
        {
            return;
        }

        throw new AuthorizationFailedException("You are not allowed to access attachments for this leave request.");
    }

    private bool IsInAnyRole(IEnumerable<string> roleNames)
    {
        return _currentUserContext.Roles.Any(currentRole =>
            roleNames.Any(roleName => string.Equals(roleName, currentRole, StringComparison.OrdinalIgnoreCase)));
    }

    private static LeaveAttachmentDto MapAttachment(LeaveAttachment attachment)
    {
        return new LeaveAttachmentDto(
            attachment.LeaveAttachmentId,
            attachment.LeaveRequestId,
            attachment.FileName,
            OperationalServiceHelpers.ToSafeRelativeFilePath(attachment.FilePath),
            attachment.FileExtension,
            attachment.FileStorageItemId,
            attachment.UploadedBy,
            attachment.UploadedByUser.Username,
            attachment.UploadedAt,
            attachment.IsActive);
    }
}
