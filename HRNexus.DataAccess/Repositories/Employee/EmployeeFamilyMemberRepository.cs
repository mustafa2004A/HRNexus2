using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Employee;

public sealed class EmployeeFamilyMemberRepository : IEmployeeFamilyMemberRepository
{
    private readonly HRNexusDbContext _dbContext;

    public EmployeeFamilyMemberRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeFamilyMemberQueryResult>> GetByEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        return await CreateQuery()
            .Where(member => member.EmployeeId == employeeId)
            .OrderBy(member => member.Person.LastName)
            .ThenBy(member => member.Person.FirstName)
            .ThenBy(member => member.FamilyMemberId)
            .Select(member => new EmployeeFamilyMemberQueryResult(
                member.FamilyMemberId,
                member.EmployeeId,
                member.PersonId,
                member.Person.FirstName,
                member.Person.SecondName,
                member.Person.ThirdName,
                member.Person.LastName,
                member.Person.FullName,
                member.Person.PreferredName,
                member.Person.DateOfBirth,
                member.Person.GenderId,
                member.Person.MaritalStatusId,
                member.Person.NationalityCountryId,
                member.Person.PhotoUrl,
                member.Person.IsDeleted,
                member.RelationshipTypeId,
                member.RelationshipType.Name))
            .ToListAsync(cancellationToken);
    }

    public Task<EmployeeFamilyMemberQueryResult?> GetByIdAsync(
        int employeeId,
        int familyMemberId,
        CancellationToken cancellationToken = default)
    {
        return CreateQuery()
            .Where(member => member.EmployeeId == employeeId && member.FamilyMemberId == familyMemberId)
            .Select(member => new EmployeeFamilyMemberQueryResult(
                member.FamilyMemberId,
                member.EmployeeId,
                member.PersonId,
                member.Person.FirstName,
                member.Person.SecondName,
                member.Person.ThirdName,
                member.Person.LastName,
                member.Person.FullName,
                member.Person.PreferredName,
                member.Person.DateOfBirth,
                member.Person.GenderId,
                member.Person.MaritalStatusId,
                member.Person.NationalityCountryId,
                member.Person.PhotoUrl,
                member.Person.IsDeleted,
                member.RelationshipTypeId,
                member.RelationshipType.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<EmployeeFamilyMember?> GetByIdForUpdateAsync(
        int employeeId,
        int familyMemberId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeFamilyMembers
            .Include(member => member.Person)
            .FirstOrDefaultAsync(member => member.EmployeeId == employeeId && member.FamilyMemberId == familyMemberId, cancellationToken);
    }

    public Task AddAsync(EmployeeFamilyMember familyMember, CancellationToken cancellationToken = default)
    {
        return _dbContext.EmployeeFamilyMembers.AddAsync(familyMember, cancellationToken).AsTask();
    }

    public void Remove(EmployeeFamilyMember familyMember)
    {
        _dbContext.EmployeeFamilyMembers.Remove(familyMember);
    }

    private IQueryable<EmployeeFamilyMember> CreateQuery()
    {
        return _dbContext.EmployeeFamilyMembers.AsNoTracking();
    }

}
