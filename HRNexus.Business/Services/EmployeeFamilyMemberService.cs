using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Core;
using HRNexus.Business.Models.Employee;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Employee;

namespace HRNexus.Business.Services;

public sealed class EmployeeFamilyMemberService : IEmployeeFamilyMemberService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeFamilyMemberRepository _familyMemberRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;

    public EmployeeFamilyMemberService(
        IEmployeeRepository employeeRepository,
        IEmployeeFamilyMemberRepository familyMemberRepository,
        IReferenceDataRepository referenceDataRepository,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext)
    {
        _employeeRepository = employeeRepository;
        _familyMemberRepository = familyMemberRepository;
        _referenceDataRepository = referenceDataRepository;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeFamilyMemberDto>> GetByEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        var members = await _familyMemberRepository.GetByEmployeeAsync(employeeId, cancellationToken);
        return members.Select(MapFamilyMember).ToList();
    }

    public async Task<EmployeeFamilyMemberDto> GetByIdAsync(
        int employeeId,
        int familyMemberId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        var member = await _familyMemberRepository.GetByIdAsync(employeeId, familyMemberId, cancellationToken)
            ?? throw FamilyMemberNotFound(familyMemberId);

        return MapFamilyMember(member);
    }

    public async Task<EmployeeFamilyMemberDto> CreateAsync(
        int employeeId,
        CreateEmployeeFamilyMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Person);

        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        await ValidatePersonReferencesAsync(request.Person, cancellationToken);
        await EnsureRelationshipTypeExistsAsync(request.RelationshipTypeId, cancellationToken);

        var person = PersonService.CreatePersonEntity(request.Person);
        person.CreatedBy = _currentUserContext.UserId;
        person.CreatedDate = DateTime.UtcNow;

        var familyMember = new EmployeeFamilyMember
        {
            EmployeeId = employeeId,
            Person = person,
            RelationshipTypeId = request.RelationshipTypeId
        };

        await _familyMemberRepository.AddAsync(familyMember, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create employee family member", cancellationToken);

        return await GetByIdAsync(employeeId, familyMember.FamilyMemberId, cancellationToken);
    }

    public async Task<EmployeeFamilyMemberDto> UpdateAsync(
        int employeeId,
        int familyMemberId,
        UpdateEmployeeFamilyMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Person);

        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        await ValidatePersonReferencesAsync(request.Person, cancellationToken);
        await EnsureRelationshipTypeExistsAsync(request.RelationshipTypeId, cancellationToken);

        var familyMember = await _familyMemberRepository.GetByIdForUpdateAsync(employeeId, familyMemberId, cancellationToken)
            ?? throw FamilyMemberNotFound(familyMemberId);

        PersonService.ApplyPersonRequest(familyMember.Person, request.Person);
        familyMember.Person.ModifiedBy = _currentUserContext.UserId;
        familyMember.Person.ModifiedDate = DateTime.UtcNow;
        familyMember.RelationshipTypeId = request.RelationshipTypeId;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update employee family member", cancellationToken);
        return await GetByIdAsync(employeeId, familyMemberId, cancellationToken);
    }

    public async Task<EmployeeFamilyMemberDto> DeleteAsync(
        int employeeId,
        int familyMemberId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        var existing = await GetByIdAsync(employeeId, familyMemberId, cancellationToken);
        var familyMember = await _familyMemberRepository.GetByIdForUpdateAsync(employeeId, familyMemberId, cancellationToken)
            ?? throw FamilyMemberNotFound(familyMemberId);

        _familyMemberRepository.Remove(familyMember);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "delete employee family relationship", cancellationToken);

        return existing;
    }

    private async Task ValidatePersonReferencesAsync(CreatePersonRequest request, CancellationToken cancellationToken)
    {
        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BusinessRuleException("Date of birth cannot be in the future.");
        }

        if (request.GenderId.HasValue && !await _referenceDataRepository.GenderExistsAsync(request.GenderId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Gender {request.GenderId.Value} was not found.");
        }

        if (request.MaritalStatusId.HasValue && !await _referenceDataRepository.MaritalStatusExistsAsync(request.MaritalStatusId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Marital status {request.MaritalStatusId.Value} was not found.");
        }

        if (request.NationalityCountryId.HasValue && !await _referenceDataRepository.CountryExistsAsync(request.NationalityCountryId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Nationality country {request.NationalityCountryId.Value} was not found.");
        }
    }

    private async Task EnsureEmployeeExistsAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (!await _employeeRepository.ExistsAsync(employeeId, cancellationToken))
        {
            throw new EntityNotFoundException($"Employee {employeeId} was not found.");
        }
    }

    private async Task EnsureRelationshipTypeExistsAsync(int relationshipTypeId, CancellationToken cancellationToken)
    {
        if (!await _referenceDataRepository.RelationshipTypeExistsAsync(relationshipTypeId, cancellationToken))
        {
            throw new BusinessRuleException($"Relationship type {relationshipTypeId} was not found or is inactive.");
        }
    }

    private static EmployeeFamilyMemberDto MapFamilyMember(EmployeeFamilyMemberQueryResult member)
    {
        var person = new PersonDto(
            member.PersonId,
            member.FirstName,
            member.SecondName,
            member.ThirdName,
            member.LastName,
            member.FullName,
            member.PreferredName,
            member.DateOfBirth,
            member.GenderId,
            member.MaritalStatusId,
            member.NationalityCountryId,
            OperationalServiceHelpers.ToSafeNullableRelativeFilePath(member.PhotoUrl),
            member.IsDeleted);

        return new EmployeeFamilyMemberDto(
            member.FamilyMemberId,
            member.EmployeeId,
            member.PersonId,
            member.FullName,
            member.RelationshipTypeId,
            member.RelationshipTypeName,
            person);
    }

    private static EntityNotFoundException FamilyMemberNotFound(int familyMemberId)
    {
        return new EntityNotFoundException($"Employee family member {familyMemberId} was not found.");
    }
}
