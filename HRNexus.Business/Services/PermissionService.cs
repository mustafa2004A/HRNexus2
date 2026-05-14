using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Auth;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;

    public PermissionService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<IReadOnlyList<string>> GetRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _userRepository.GetRoleNamesAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<UserPermissionDto>> GetPermissionSummariesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _userRepository.GetPermissionSummariesAsync(userId, cancellationToken);

        return permissions
            .Select(permission => new UserPermissionDto(
                permission.ModuleName,
                permission.PermissionMask,
                permission.Source))
            .ToList();
    }

    public async Task<bool> IsInRoleAsync(int userId, string roleName, CancellationToken cancellationToken = default)
    {
        var roles = await GetRolesAsync(userId, cancellationToken);
        return roles.Any(role => string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase));
    }
}
