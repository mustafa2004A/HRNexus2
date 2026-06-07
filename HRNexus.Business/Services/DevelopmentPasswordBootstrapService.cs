using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Auth;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.Business.Services;

public sealed class DevelopmentPasswordBootstrapService : IDevelopmentPasswordBootstrapService
{
    private const string ActiveAccountStatusCode = "A";
    private const string DisabledAccountStatusCode = "D";
    private const string AdminUsername = "Admin";
    private const string LegacyAdminUsername = "admin";

    private static readonly DemoAccountDefinition[] DemoAccounts =
    [
        new(AdminUsername, "HRN-ADMIN", "Admin", "Full system administration role."),
        new("sarah.haddad", "HRN-EMP-0001", "HRManager", "HR management role for employee administration."),
        new("omar.khalil", "HRN-EMP-0002", "DepartmentManager", "Department-level manager role."),
        new("lina.nasser", "HRN-EMP-0003", "HRClerk", "HR operations role for routine employee data entry."),
        new("nadia.saleh", "HRN-EMP-0004", "Employee", "Standard employee self-service role.")
    ];

    private readonly HRNexusDbContext _dbContext;
    private readonly IPasswordHashingService _passwordHashingService;

    public DevelopmentPasswordBootstrapService(
        HRNexusDbContext dbContext,
        IPasswordHashingService passwordHashingService)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<DevelopmentPasswordBootstrapResultDto> ReseedDemoPasswordsAsync(
        DevelopmentPasswordBootstrapRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var credentials = BuildCredentialRequests(request);

        if (credentials.Count == 0)
        {
            throw new BusinessRuleException("At least one valid username is required.");
        }

        var activeStatus = await _dbContext.AccountStatuses
            .FirstOrDefaultAsync(status => status.StatusCode == ActiveAccountStatusCode, cancellationToken)
            ?? throw new BusinessRuleException("Active account status was not found.");

        var disabledStatus = await _dbContext.AccountStatuses
            .FirstOrDefaultAsync(status => status.StatusCode == DisabledAccountStatusCode, cancellationToken);

        var result = new DevelopmentPasswordBootstrapAccumulator();
        var roleLookup = await EnsureDemoRolesAsync(cancellationToken);
        result.RoleAssignmentsEnsured.AddRange(await EnsureDemoRolePermissionsAsync(roleLookup, cancellationToken));

        var requestedUsernames = credentials
            .Select(credential => credential.Username)
            .Append(AdminUsername)
            .Append(LegacyAdminUsername)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var lookupUsernames = requestedUsernames
            .Select(username => username.ToUpperInvariant())
            .ToArray();

        var existingUsers = await _dbContext.Users
            .Include(user => user.UserRoles)
            .Where(user => lookupUsernames.Contains(user.Username.ToUpper()))
            .OrderBy(user => user.UserId)
            .ToListAsync(cancellationToken);

        var demoDefinitions = DemoAccounts.ToDictionary(
            account => account.Username,
            StringComparer.OrdinalIgnoreCase);

        var employeeCodes = credentials
            .Select(credential => GetDemoDefinition(credential.Username))
            .Where(definition => definition is not null)
            .Select(definition => definition!.EmployeeCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var employees = await _dbContext.Employees
            .Include(employee => employee.Person)
            .Where(employee => employeeCodes.Contains(employee.EmployeeCode))
            .ToDictionaryAsync(employee => employee.EmployeeCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var employeeCode in employeeCodes.Where(employeeCode => !employees.ContainsKey(employeeCode)))
        {
            result.MissingEmployeeCodes.Add(employeeCode);
        }

        ReactivateDemoEmployees(employees.Values, result);

        var targetEmployeeIds = employees.Values
            .Select(employee => employee.EmployeeId)
            .Distinct()
            .ToArray();

        var usersWithTargetEmployees = await _dbContext.Users
            .Where(user => user.EmployeeId.HasValue && targetEmployeeIds.Contains(user.EmployeeId.Value))
            .OrderBy(user => user.UserId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var touchedUsers = new List<User>();

        foreach (var credential in credentials)
        {
            var canonicalUsername = GetCanonicalDemoUsername(credential.Username);
            var demoDefinition = GetDemoDefinition(canonicalUsername);
            var matchingUsers = existingUsers
                .Where(user => string.Equals(user.Username, canonicalUsername, StringComparison.OrdinalIgnoreCase))
                .OrderBy(user => user.UserId)
                .ToList();

            var primaryUser = SelectPrimaryUser(matchingUsers, canonicalUsername);

            if (matchingUsers.Count > 1)
            {
                result.DuplicateUsernamesFound.Add(canonicalUsername);
            }

            var employee = demoDefinition is null || !employees.TryGetValue(demoDefinition.EmployeeCode, out var foundEmployee)
                ? null
                : foundEmployee;

            foreach (var duplicate in matchingUsers.Where(user => user != primaryUser))
            {
                DeactivateDuplicateUser(duplicate, disabledStatus, now);
                result.DeactivatedDuplicateUsernames.Add($"{duplicate.Username}#{duplicate.UserId}");

                if (employee is not null && duplicate.EmployeeId == employee.EmployeeId)
                {
                    duplicate.EmployeeId = null;
                }
            }

            if (primaryUser is null)
            {
                if (demoDefinition is null)
                {
                    result.MissingUsernames.Add(canonicalUsername);
                    continue;
                }

                primaryUser = new User
                {
                    EmployeeId = GetSafeEmployeeId(employee, null, usersWithTargetEmployees, result, demoDefinition, canonicalUsername),
                    Username = canonicalUsername,
                    PasswordHash = _passwordHashingService.HashPassword(credential.Password),
                    IsActive = true,
                    FailedLoginAttempts = 0,
                    AccountStatusId = activeStatus.AccountStatusId,
                    CreatedDate = now,
                    ModifiedDate = now
                };

                await _dbContext.Users.AddAsync(primaryUser, cancellationToken);
                existingUsers.Add(primaryUser);
                touchedUsers.Add(primaryUser);
                result.CreatedUsernames.Add(canonicalUsername);
                result.UpdatedUsernames.Add(canonicalUsername);
                continue;
            }

            if (!string.Equals(primaryUser.Username, canonicalUsername, StringComparison.Ordinal))
            {
                result.NormalizedUsernames.Add($"{primaryUser.Username} -> {canonicalUsername}");
                primaryUser.Username = canonicalUsername;
            }

            primaryUser.PasswordHash = _passwordHashingService.HashPassword(credential.Password);
            primaryUser.IsActive = true;
            primaryUser.FailedLoginAttempts = 0;
            primaryUser.AccountStatusId = activeStatus.AccountStatusId;
            primaryUser.ModifiedDate = GetSafeModifiedDate(primaryUser.CreatedDate, now);

            if (demoDefinition is not null)
            {
                primaryUser.EmployeeId = GetSafeEmployeeId(
                    employee,
                    primaryUser,
                    usersWithTargetEmployees,
                    result,
                    demoDefinition,
                    canonicalUsername);
            }

            touchedUsers.Add(primaryUser);
            result.UpdatedUsernames.Add(canonicalUsername);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var adminUserId = touchedUsers
            .Concat(existingUsers)
            .Where(user => string.Equals(user.Username, AdminUsername, StringComparison.OrdinalIgnoreCase))
            .Select(user => (int?)user.UserId)
            .FirstOrDefault();

        foreach (var user in touchedUsers)
        {
            var demoDefinition = demoDefinitions.GetValueOrDefault(user.Username);

            if (demoDefinition is null)
            {
                continue;
            }

            if (!roleLookup.TryGetValue(demoDefinition.RoleName, out var role))
            {
                result.MissingRoleNames.Add(demoDefinition.RoleName);
                continue;
            }

            if (user.UserRoles.Any(userRole => userRole.RoleId == role.RoleId)
                || await _dbContext.UserRoles.AnyAsync(
                    userRole => userRole.UserId == user.UserId && userRole.RoleId == role.RoleId,
                    cancellationToken))
            {
                continue;
            }

            await _dbContext.UserRoles.AddAsync(
                new UserRole
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId,
                    AssignedBy = user.UserId == adminUserId ? null : adminUserId,
                    AssignedDate = now
                },
                cancellationToken);

            result.RoleAssignmentsEnsured.Add($"{user.Username}:{role.RoleName}");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return result.ToDto(DateTime.UtcNow);
    }

    private async Task<Dictionary<string, Role>> EnsureDemoRolesAsync(CancellationToken cancellationToken)
    {
        var roleNames = DemoAccounts
            .Select(account => account.RoleName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existingRoles = await _dbContext.Roles
            .Where(role => roleNames.Contains(role.RoleName))
            .ToListAsync(cancellationToken);

        var roleLookup = existingRoles.ToDictionary(role => role.RoleName, StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var account in DemoAccounts)
        {
            if (roleLookup.TryGetValue(account.RoleName, out var role))
            {
                role.IsActive = true;

                if (string.IsNullOrWhiteSpace(role.RoleDescription))
                {
                    role.RoleDescription = account.RoleDescription;
                }

                continue;
            }

            role = new Role
            {
                RoleName = account.RoleName,
                RoleDescription = account.RoleDescription,
                IsActive = true,
                CreatedDate = now
            };

            await _dbContext.Roles.AddAsync(role, cancellationToken);
            roleLookup[role.RoleName] = role;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return roleLookup;
    }

    private async Task<IReadOnlyList<string>> EnsureDemoRolePermissionsAsync(
        IReadOnlyDictionary<string, Role> roleLookup,
        CancellationToken cancellationToken)
    {
        var ensured = new List<string>();
        var modules = await _dbContext.Modules
            .Where(module => module.IsActive)
            .OrderBy(module => module.ModuleName)
            .ToDictionaryAsync(module => module.ModuleName, StringComparer.OrdinalIgnoreCase, cancellationToken);

        if (modules.Count == 0)
        {
            return ensured;
        }

        var permissionBits = await _dbContext.Permissions
            .Where(permission => permission.IsActive)
            .ToDictionaryAsync(permission => permission.PermissionName, permission => permission.BitValue, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var specs = new List<RolePermissionSpec>();

        if (roleLookup.TryGetValue("Admin", out var adminRole))
        {
            specs.AddRange(modules.Values.Select(module => new RolePermissionSpec(adminRole.RoleName, module.ModuleName, -1)));
        }

        specs.AddRange(
        [
            new("HRManager", "Dashboard", -1),
            new("HRManager", "Core", -1),
            new("HRManager", "People", -1),
            new("HRManager", "Organization", -1),
            new("HRManager", "Employee", -1),
            new("HRManager", "Leave", -1),
            new("HRManager", "LeaveRequests", -1),
            new("HRManager", "LeaveBalances", -1),
            new("HRManager", "Documents", -1),
            new("HRManager", "Holidays", -1),
            new("HRManager", "FileStorage", PermissionMask(permissionBits, "Read", "Upload", "Download", "VerifyIntegrity")),
            new("HRClerk", "Dashboard", PermissionMask(permissionBits, "Read")),
            new("HRClerk", "Core", PermissionMask(permissionBits, "Read", "Create", "Update")),
            new("HRClerk", "People", PermissionMask(permissionBits, "Read", "Create", "Update")),
            new("HRClerk", "Employee", PermissionMask(permissionBits, "Read", "Create", "Update")),
            new("HRClerk", "Documents", PermissionMask(permissionBits, "Read", "Create", "Update", "Upload", "Download")),
            new("HRClerk", "Leave", PermissionMask(permissionBits, "Read", "Create", "Update")),
            new("HRClerk", "LeaveRequests", PermissionMask(permissionBits, "Read", "Create", "Update")),
            new("HRClerk", "LeaveBalances", PermissionMask(permissionBits, "Read")),
            new("DepartmentManager", "Dashboard", PermissionMask(permissionBits, "Read")),
            new("DepartmentManager", "Employee", PermissionMask(permissionBits, "Read", "Approve")),
            new("DepartmentManager", "Leave", PermissionMask(permissionBits, "Read", "Approve")),
            new("DepartmentManager", "LeaveRequests", PermissionMask(permissionBits, "Read", "Approve")),
            new("DepartmentManager", "LeaveBalances", PermissionMask(permissionBits, "Read")),
            new("Employee", "Employee", PermissionMask(permissionBits, "Read", "ViewOwn")),
            new("Employee", "Leave", PermissionMask(permissionBits, "Read", "Create", "ViewOwn", "ManageOwn")),
            new("Employee", "LeaveRequests", PermissionMask(permissionBits, "Read", "Create", "ViewOwn", "ManageOwn")),
            new("Employee", "LeaveBalances", PermissionMask(permissionBits, "Read", "ViewOwn")),
            new("Employee", "Documents", PermissionMask(permissionBits, "Read", "Upload", "Download", "ViewOwn"))
        ]);

        var roleIds = roleLookup.Values.Select(role => role.RoleId).ToArray();
        var moduleIds = modules.Values.Select(module => module.ModuleId).ToArray();
        var existingPermissions = await _dbContext.RolePermissions
            .Where(permission => roleIds.Contains(permission.RoleId) && moduleIds.Contains(permission.ModuleId))
            .ToListAsync(cancellationToken);

        foreach (var spec in specs)
        {
            if (!roleLookup.TryGetValue(spec.RoleName, out var role)
                || !modules.TryGetValue(spec.ModuleName, out var module))
            {
                continue;
            }

            var existing = existingPermissions.FirstOrDefault(permission =>
                permission.RoleId == role.RoleId && permission.ModuleId == module.ModuleId);

            if (existing is null)
            {
                existing = new RolePermission
                {
                    RoleId = role.RoleId,
                    ModuleId = module.ModuleId,
                    PermissionMask = spec.PermissionMask
                };

                await _dbContext.RolePermissions.AddAsync(existing, cancellationToken);
                existingPermissions.Add(existing);
            }
            else
            {
                existing.PermissionMask = spec.PermissionMask;
            }

            ensured.Add($"{role.RoleName}:{module.ModuleName}={spec.PermissionMask}");
        }

        return ensured;
    }

    private static IReadOnlyList<DevelopmentCredential> BuildCredentialRequests(DevelopmentPasswordBootstrapRequest request)
    {
        var credentials = new Dictionary<string, DevelopmentCredential>(StringComparer.OrdinalIgnoreCase);

        if (request.Credentials.Count > 0)
        {
            foreach (var credential in request.Credentials)
            {
                var username = NormalizeUsername(credential.Username);
                var canonicalUsername = GetCanonicalDemoUsername(username);

                ValidatePassword(credential.Password);
                AddCredential(credentials, canonicalUsername, credential.Password);
            }

            return credentials.Values.ToArray();
        }

        var sharedPassword = request.Password;

        if (string.IsNullOrWhiteSpace(sharedPassword))
        {
            throw new BusinessRuleException("Development password is required.");
        }

        var requestedUsernames = NormalizeUsernames(request.Usernames is { Count: > 0 }
            ? request.Usernames
            : DemoAccounts.Select(account => account.Username));

        foreach (var username in requestedUsernames)
        {
            var canonicalUsername = GetCanonicalDemoUsername(username);
            ValidatePassword(sharedPassword);
            AddCredential(credentials, canonicalUsername, sharedPassword);
        }

        return credentials.Values.ToArray();
    }

    private static void AddCredential(
        IDictionary<string, DevelopmentCredential> credentials,
        string username,
        string password)
    {
        if (credentials.TryGetValue(username, out var existing)
            && !string.Equals(existing.Password, password, StringComparison.Ordinal))
        {
            throw new BusinessRuleException($"Duplicate password bootstrap entries for {username} must use the same password.");
        }

        credentials[username] = new DevelopmentCredential(username, password);
    }

    private static int? GetSafeEmployeeId(
        DataAccess.Entities.Employee.Employee? employee,
        User? currentUser,
        IReadOnlyList<User> usersWithTargetEmployees,
        DevelopmentPasswordBootstrapAccumulator result,
        DemoAccountDefinition demoDefinition,
        string username)
    {
        if (employee is null)
        {
            return currentUser?.EmployeeId;
        }

        var conflictingUser = usersWithTargetEmployees.FirstOrDefault(user =>
            user.EmployeeId == employee.EmployeeId
            && (currentUser is null || user.UserId != currentUser.UserId)
            && !string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));

        if (conflictingUser is not null)
        {
            result.Warnings.Add(
                $"{username} was not linked to {demoDefinition.EmployeeCode} because that employee is already linked to {conflictingUser.Username}#{conflictingUser.UserId}.");

            return currentUser?.EmployeeId;
        }

        return employee.EmployeeId;
    }

    private static void ReactivateDemoEmployees(
        IEnumerable<DataAccess.Entities.Employee.Employee> employees,
        DevelopmentPasswordBootstrapAccumulator result)
    {
        foreach (var employee in employees)
        {
            if (employee.IsDeleted)
            {
                employee.IsDeleted = false;
                employee.DeletedBy = null;
                employee.DeletedDate = null;
                employee.ModifiedDate = DateTime.UtcNow;
                result.Warnings.Add($"{employee.EmployeeCode} was reactivated because it is linked to a demo login account.");
            }

            if (employee.Person.IsDeleted)
            {
                employee.Person.IsDeleted = false;
                employee.Person.DeletedBy = null;
                employee.Person.DeletedDate = null;
                employee.Person.ModifiedDate = DateTime.UtcNow;
                result.Warnings.Add($"{employee.EmployeeCode} person row was reactivated because it is linked to a demo login account.");
            }
        }
    }

    private static User? SelectPrimaryUser(IReadOnlyList<User> users, string canonicalUsername)
    {
        if (users.Count == 0)
        {
            return null;
        }

        return users.FirstOrDefault(user => string.Equals(user.Username, canonicalUsername, StringComparison.Ordinal))
            ?? users.First();
    }

    private static void DeactivateDuplicateUser(User user, AccountStatus? disabledStatus, DateTime now)
    {
        user.IsActive = false;
        user.FailedLoginAttempts = 0;
        user.ModifiedDate = GetSafeModifiedDate(user.CreatedDate, now);

        if (disabledStatus is not null)
        {
            user.AccountStatusId = disabledStatus.AccountStatusId;
        }
    }

    private static DemoAccountDefinition? GetDemoDefinition(string username)
    {
        return DemoAccounts.FirstOrDefault(account =>
            string.Equals(account.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> NormalizeUsernames(IEnumerable<string> usernames)
    {
        return usernames
            .Select(NormalizeUsername)
            .Where(username => !string.IsNullOrWhiteSpace(username))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeUsername(string username)
    {
        return BusinessValidation.NormalizeRequiredText(username, "Username");
    }

    private static string GetCanonicalDemoUsername(string username)
    {
        var demoDefinition = GetDemoDefinition(username);

        if (demoDefinition is not null)
        {
            return demoDefinition.Username;
        }

        return string.Equals(username, LegacyAdminUsername, StringComparison.OrdinalIgnoreCase)
            ? AdminUsername
            : username;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new BusinessRuleException("Password is required.");
        }

        if (password.Length < 10)
        {
            throw new BusinessRuleException("Development password must be at least 10 characters long.");
        }

        if (password.Any(char.IsWhiteSpace))
        {
            throw new BusinessRuleException("Development password must not contain whitespace.");
        }

        if (!password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit)
            || !password.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new BusinessRuleException("Development password must include uppercase, lowercase, digit, and symbol characters.");
        }
    }

    private static int PermissionMask(IReadOnlyDictionary<string, int> permissionBits, params string[] permissionNames)
    {
        var mask = 0;

        foreach (var permissionName in permissionNames)
        {
            if (permissionBits.TryGetValue(permissionName, out var bitValue) && bitValue > 0)
            {
                mask |= bitValue;
            }
        }

        return mask;
    }

    private static DateTime GetSafeModifiedDate(DateTime createdDate, DateTime now)
    {
        return createdDate > now ? createdDate : now;
    }

    private sealed record DemoAccountDefinition(
        string Username,
        string EmployeeCode,
        string RoleName,
        string RoleDescription);

    private sealed record DevelopmentCredential(string Username, string Password);

    private sealed record RolePermissionSpec(string RoleName, string ModuleName, int PermissionMask);

    private sealed class DevelopmentPasswordBootstrapAccumulator
    {
        public List<string> UpdatedUsernames { get; } = [];
        public List<string> MissingUsernames { get; } = [];
        public List<string> CreatedUsernames { get; } = [];
        public List<string> NormalizedUsernames { get; } = [];
        public List<string> DuplicateUsernamesFound { get; } = [];
        public List<string> DeactivatedDuplicateUsernames { get; } = [];
        public List<string> RoleAssignmentsEnsured { get; } = [];
        public List<string> MissingEmployeeCodes { get; } = [];
        public List<string> MissingRoleNames { get; } = [];
        public List<string> Warnings { get; } = [];

        public DevelopmentPasswordBootstrapResultDto ToDto(DateTime generatedAt)
        {
            var updatedUsernames = UniqueSorted(UpdatedUsernames);
            var createdUsernames = UniqueSorted(CreatedUsernames);

            return new DevelopmentPasswordBootstrapResultDto(
                updatedUsernames.Count,
                updatedUsernames,
                UniqueSorted(MissingUsernames),
                generatedAt,
                createdUsernames.Count,
                createdUsernames,
                UniqueSorted(NormalizedUsernames),
                UniqueSorted(DuplicateUsernamesFound),
                UniqueSorted(DeactivatedDuplicateUsernames),
                UniqueSorted(RoleAssignmentsEnsured),
                UniqueSorted(MissingEmployeeCodes),
                UniqueSorted(MissingRoleNames),
                UniqueSorted(Warnings));
        }

        private static IReadOnlyList<string> UniqueSorted(IEnumerable<string> values)
        {
            return values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
