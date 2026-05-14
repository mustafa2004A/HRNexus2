using HRNexus.Business.Models.Auth;

namespace HRNexus.Business.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<string>> GetRolesAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserPermissionDto>> GetPermissionSummariesAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default);
}
