namespace HRNexus.DataAccess.Entities.Security;

public sealed class RolePermission
{
    public int RoleId { get; set; }
    public int ModuleId { get; set; }
    public int PermissionMask { get; set; }

    public Role Role { get; set; } = null!;
    public Module Module { get; set; } = null!;
}

public sealed class UserPermission
{
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public int PermissionMask { get; set; }

    public User User { get; set; } = null!;
    public Module Module { get; set; } = null!;
}

public sealed class PermissionAudit
{
    public int AuditId { get; set; }
    public int RoleId { get; set; }
    public int ModuleId { get; set; }
    public int OldMask { get; set; }
    public int NewMask { get; set; }
    public int ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; }

    public Role Role { get; set; } = null!;
    public Module Module { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
