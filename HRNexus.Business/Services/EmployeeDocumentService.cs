using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Employee;
using HRNexus.Business.Models.Files;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Employee;

namespace HRNexus.Business.Services;

public sealed class EmployeeDocumentService : IEmployeeDocumentService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeDocumentRepository _employeeDocumentRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;

    public EmployeeDocumentService(
        IEmployeeRepository employeeRepository,
        IEmployeeDocumentRepository employeeDocumentRepository,
        IReferenceDataRepository referenceDataRepository,
        IFileStorageService fileStorageService,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext)
    {
        _employeeRepository = employeeRepository;
        _employeeDocumentRepository = employeeDocumentRepository;
        _referenceDataRepository = referenceDataRepository;
        _fileStorageService = fileStorageService;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeDocumentItemDto>> GetEmployeeDocumentsAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);

        var documents = await _employeeDocumentRepository.GetByEmployeeAsync(employeeId, cancellationToken);
        return documents.Select(MapDocumentItem).ToList();
    }

    public async Task<EmployeeDocumentDto> GetByIdAsync(
        int employeeId,
        int documentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        var document = await _employeeDocumentRepository.GetByIdAsync(employeeId, documentId, cancellationToken)
            ?? throw DocumentNotFound(documentId);

        return OperationalServiceHelpers.ToEmployeeDocumentDto(document);
    }

    public async Task<EmployeeDocumentDto> UploadAsync(
        int employeeId,
        UploadEmployeeDocumentRequest request,
        FileUploadContent file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(file);
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        await ValidateDocumentTypeAndDatesAsync(request.DocumentTypeId, request.IssueDate, request.ExpiryDate, cancellationToken);

        var uploadedByUserId = _currentUserContext.UserId
            ?? throw new AuthorizationFailedException("Authenticated user is required to upload employee documents.");

        var storedFile = await _fileStorageService.SaveAsync(
            FileStorageCategories.EmployeeDocument,
            file,
            uploadedByUserId,
            cancellationToken);

        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentTypeId = request.DocumentTypeId,
            FileStorageItemId = storedFile.FileStorageItemId,
            DocumentName = OperationalServiceHelpers.RequiredText(request.DocumentName, "Document name"),
            ReferenceNumber = OperationalServiceHelpers.OptionalText(request.ReferenceNumber),
            FilePath = storedFile.RelativePath,
            FileExtension = storedFile.FileExtension,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            IsVerified = false,
            VerifiedBy = null,
            VerifiedDate = null,
            IsActive = true,
            UploadedBy = uploadedByUserId,
            UploadedDate = DateTime.UtcNow,
            Remarks = OperationalServiceHelpers.OptionalText(request.Remarks)
        };

        await _employeeDocumentRepository.AddAsync(document, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "upload employee document", cancellationToken);

        return await GetByIdAsync(employeeId, document.DocumentId, cancellationToken);
    }

    public async Task<EmployeeDocumentDto> UpdateAsync(
        int employeeId,
        int documentId,
        UpdateEmployeeDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        await ValidateDocumentTypeAndDatesAsync(request.DocumentTypeId, request.IssueDate, request.ExpiryDate, cancellationToken);

        var document = await _employeeDocumentRepository.GetByIdForUpdateAsync(employeeId, documentId, cancellationToken)
            ?? throw DocumentNotFound(documentId);

        document.DocumentTypeId = request.DocumentTypeId;
        document.DocumentName = OperationalServiceHelpers.RequiredText(request.DocumentName, "Document name");
        document.ReferenceNumber = OperationalServiceHelpers.OptionalText(request.ReferenceNumber);
        document.IssueDate = request.IssueDate;
        document.ExpiryDate = request.ExpiryDate;
        document.IsActive = request.IsActive;
        document.Remarks = OperationalServiceHelpers.OptionalText(request.Remarks);

        if (request.IsVerified)
        {
            document.IsVerified = true;
            document.VerifiedBy = request.VerifiedBy ?? _currentUserContext.UserId;
            document.VerifiedDate = request.VerifiedDate ?? DateTime.UtcNow;

            if (document.VerifiedBy.HasValue
                && !await _referenceDataRepository.UserExistsAsync(document.VerifiedBy.Value, cancellationToken))
            {
                throw new BusinessRuleException($"Verifier user {document.VerifiedBy.Value} was not found.");
            }
        }
        else
        {
            document.IsVerified = false;
            document.VerifiedBy = null;
            document.VerifiedDate = null;
        }

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update employee document", cancellationToken);
        return await GetByIdAsync(employeeId, documentId, cancellationToken);
    }

    public async Task<EmployeeDocumentDto> DeleteAsync(
        int employeeId,
        int documentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        var document = await _employeeDocumentRepository.GetByIdForUpdateAsync(employeeId, documentId, cancellationToken)
            ?? throw DocumentNotFound(documentId);

        document.IsActive = false;
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "deactivate employee document", cancellationToken);

        return await GetByIdAsync(employeeId, documentId, cancellationToken);
    }

    private async Task ValidateDocumentTypeAndDatesAsync(
        int documentTypeId,
        DateOnly? issueDate,
        DateOnly? expiryDate,
        CancellationToken cancellationToken)
    {
        var documentType = await _referenceDataRepository.GetDocumentTypeAsync(documentTypeId, cancellationToken)
            ?? throw new BusinessRuleException($"Document type {documentTypeId} was not found.");

        if (!documentType.IsActive)
        {
            throw new BusinessRuleException($"Document type {documentTypeId} is inactive.");
        }

        if (documentType.IsExpiryTracked && !expiryDate.HasValue)
        {
            throw new BusinessRuleException("Expiry date is required for this document type.");
        }

        if (issueDate.HasValue && expiryDate.HasValue && expiryDate.Value < issueDate.Value)
        {
            throw new BusinessRuleException("Document expiry date cannot be earlier than issue date.");
        }
    }

    private async Task EnsureEmployeeExistsAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (!await _employeeRepository.ExistsAsync(employeeId, cancellationToken))
        {
            throw new EntityNotFoundException($"Employee {employeeId} was not found.");
        }
    }

    private static EntityNotFoundException DocumentNotFound(int documentId)
    {
        return new EntityNotFoundException($"Employee document {documentId} was not found.");
    }

    private static EmployeeDocumentItemDto MapDocumentItem(EmployeeDocumentItemQueryResult document)
    {
        return new EmployeeDocumentItemDto(
            document.DocumentId,
            document.DocumentName,
            document.DocumentTypeName,
            document.ReferenceNumber,
            document.FileExtension,
            document.FileStorageItemId,
            document.IssueDate,
            document.ExpiryDate,
            document.IsVerified,
            document.VerifiedByUsername,
            document.VerifiedDate,
            document.IsActive,
            document.UploadedDate);
    }
}
