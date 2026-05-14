namespace HRNexus.DataAccess.Entities.Security;

public sealed class AccountStatus
{
    public int AccountStatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}

public sealed class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public sealed class Module
{
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

public sealed class Permission
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public int BitValue { get; set; }
    public int BitOrder { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
}

public sealed class ActivityType
{
    public int ActivityTypeId { get; set; }
    public string ActivityTypeName { get; set; } = string.Empty;
    public string ActivityTypeCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<UserActivityLog> UserActivityLogs { get; set; } = new List<UserActivityLog>();
}
