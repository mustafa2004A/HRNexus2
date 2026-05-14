using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Lookup;

public sealed class ReferenceDataRepository : IReferenceDataRepository
{
    private readonly HRNexusDbContext _dbContext;

    public ReferenceDataRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> GenderExistsAsync(int genderId, CancellationToken cancellationToken = default) =>
        _dbContext.Genders.AsNoTracking().AnyAsync(gender => gender.GenderId == genderId, cancellationToken);

    public Task<bool> MaritalStatusExistsAsync(int maritalStatusId, CancellationToken cancellationToken = default) =>
        _dbContext.MaritalStatuses.AsNoTracking().AnyAsync(status => status.MaritalStatusId == maritalStatusId, cancellationToken);

    public Task<bool> CountryExistsAsync(int countryId, CancellationToken cancellationToken = default) =>
        _dbContext.Countries.AsNoTracking().AnyAsync(country => country.CountryId == countryId, cancellationToken);

    public Task<bool> ContactTypeExistsAsync(int contactTypeId, CancellationToken cancellationToken = default) =>
        _dbContext.ContactTypes.AsNoTracking().AnyAsync(type => type.ContactTypeId == contactTypeId, cancellationToken);

    public Task<bool> CityExistsAsync(int cityId, CancellationToken cancellationToken = default) =>
        _dbContext.Cities.AsNoTracking().AnyAsync(city => city.CityId == cityId, cancellationToken);

    public Task<bool> AddressTypeExistsAsync(int addressTypeId, CancellationToken cancellationToken = default) =>
        _dbContext.AddressTypes.AsNoTracking().AnyAsync(type => type.AddressTypeId == addressTypeId, cancellationToken);

    public Task<bool> IdentifierTypeExistsAsync(int identifierTypeId, CancellationToken cancellationToken = default) =>
        _dbContext.IdentifierTypes.AsNoTracking().AnyAsync(type => type.IdentifierTypeId == identifierTypeId, cancellationToken);

    public Task<bool> EmploymentStatusExistsAsync(int employmentStatusId, CancellationToken cancellationToken = default) =>
        _dbContext.EmploymentStatuses.AsNoTracking().AnyAsync(status => status.EmploymentStatusId == employmentStatusId, cancellationToken);

    public Task<bool> TerminationReasonExistsAsync(int terminationReasonId, CancellationToken cancellationToken = default) =>
        _dbContext.TerminationReasons.AsNoTracking().AnyAsync(reason => reason.TerminationReasonId == terminationReasonId, cancellationToken);

    public Task<bool> DepartmentExistsAsync(int departmentId, CancellationToken cancellationToken = default) =>
        _dbContext.Departments.AsNoTracking().AnyAsync(department => department.DepartmentId == departmentId && department.IsActive, cancellationToken);

    public Task<bool> PositionExistsAsync(int positionId, CancellationToken cancellationToken = default) =>
        _dbContext.Positions.AsNoTracking().AnyAsync(position => position.PositionId == positionId && position.IsActive, cancellationToken);

    public Task<bool> EmploymentTypeExistsAsync(int employmentTypeId, CancellationToken cancellationToken = default) =>
        _dbContext.EmploymentTypes.AsNoTracking().AnyAsync(type => type.EmploymentTypeId == employmentTypeId, cancellationToken);

    public Task<bool> JobGradeExistsAsync(int jobGradeId, CancellationToken cancellationToken = default) =>
        _dbContext.JobGrades.AsNoTracking().AnyAsync(grade => grade.JobGradeId == jobGradeId, cancellationToken);

    public Task<bool> RelationshipTypeExistsAsync(int relationshipTypeId, CancellationToken cancellationToken = default) =>
        _dbContext.RelationshipTypes.AsNoTracking().AnyAsync(type => type.RelationshipTypeId == relationshipTypeId && type.IsActive, cancellationToken);

    public Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AsNoTracking().AnyAsync(user => user.UserId == userId, cancellationToken);

    public Task<DocumentTypeReference?> GetDocumentTypeAsync(int documentTypeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.DocumentTypes
            .AsNoTracking()
            .Where(type => type.DocumentTypeId == documentTypeId)
            .Select(type => new DocumentTypeReference(type.DocumentTypeId, type.Name, type.IsExpiryTracked, type.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
