using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Employee;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IEmployeeFamilyMemberRepository
{
    Task<IReadOnlyList<EmployeeFamilyMemberQueryResult>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeFamilyMemberQueryResult?> GetByIdAsync(int employeeId, int familyMemberId, CancellationToken cancellationToken = default);
    Task<EmployeeFamilyMember?> GetByIdForUpdateAsync(int employeeId, int familyMemberId, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeFamilyMember familyMember, CancellationToken cancellationToken = default);
    void Remove(EmployeeFamilyMember familyMember);
}
