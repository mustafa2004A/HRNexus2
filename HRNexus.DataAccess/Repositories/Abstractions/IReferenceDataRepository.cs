namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IReferenceDataRepository
{
    Task<bool> GenderExistsAsync(int genderId, CancellationToken cancellationToken = default);
    Task<bool> MaritalStatusExistsAsync(int maritalStatusId, CancellationToken cancellationToken = default);
    Task<bool> CountryExistsAsync(int countryId, CancellationToken cancellationToken = default);
    Task<bool> ContactTypeExistsAsync(int contactTypeId, CancellationToken cancellationToken = default);
    Task<bool> CityExistsAsync(int cityId, CancellationToken cancellationToken = default);
    Task<bool> AddressTypeExistsAsync(int addressTypeId, CancellationToken cancellationToken = default);
    Task<bool> IdentifierTypeExistsAsync(int identifierTypeId, CancellationToken cancellationToken = default);
    Task<bool> EmploymentStatusExistsAsync(int employmentStatusId, CancellationToken cancellationToken = default);
    Task<bool> TerminationReasonExistsAsync(int terminationReasonId, CancellationToken cancellationToken = default);
    Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<bool> PositionExistsAsync(int positionId, CancellationToken cancellationToken = default);
    Task<bool> EmploymentTypeExistsAsync(int employmentTypeId, CancellationToken cancellationToken = default);
    Task<bool> JobGradeExistsAsync(int jobGradeId, CancellationToken cancellationToken = default);
    Task<bool> RelationshipTypeExistsAsync(int relationshipTypeId, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);
    Task<DocumentTypeReference?> GetDocumentTypeAsync(int documentTypeId, CancellationToken cancellationToken = default);
}

public sealed record DocumentTypeReference(int DocumentTypeId, string Name, bool IsExpiryTracked, bool IsActive);
