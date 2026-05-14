using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Security;

namespace HRNexus.DataAccess.Repositories.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdForUpdateAsync(int userId, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameForUpdateAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByUsernamesForUpdateAsync(IReadOnlyCollection<string> usernames, CancellationToken cancellationToken = default);
    Task<UserAuthQueryResult?> GetAuthByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserIdentityQueryResult?> GetIdentityByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserPermissionQueryResult>> GetPermissionSummariesAsync(int userId, CancellationToken cancellationToken = default);
    Task<AccountStatus?> GetAccountStatusByCodeAsync(string statusCode, CancellationToken cancellationToken = default);
}
