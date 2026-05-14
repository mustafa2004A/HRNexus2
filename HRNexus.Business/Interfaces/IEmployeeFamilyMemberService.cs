using HRNexus.Business.Models.Employee;

namespace HRNexus.Business.Interfaces;

public interface IEmployeeFamilyMemberService
{
    Task<IReadOnlyList<EmployeeFamilyMemberDto>> GetByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeFamilyMemberDto> GetByIdAsync(int employeeId, int familyMemberId, CancellationToken cancellationToken = default);
    Task<EmployeeFamilyMemberDto> CreateAsync(int employeeId, CreateEmployeeFamilyMemberRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeFamilyMemberDto> UpdateAsync(int employeeId, int familyMemberId, UpdateEmployeeFamilyMemberRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeFamilyMemberDto> DeleteAsync(int employeeId, int familyMemberId, CancellationToken cancellationToken = default);
}
