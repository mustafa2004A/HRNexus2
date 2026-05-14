using HRNexus.DataAccess.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRNexus.DataAccess.Configurations.Security;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "sec");
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LastLoginAt).HasColumnType("datetime2");
        builder.Property(x => x.AccountStatusId).HasColumnName("AccountStatusID");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2");

        builder.HasOne(x => x.Employee)
            .WithOne(x => x.User)
            .HasForeignKey<User>(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AccountStatus)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.AccountStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AccountStatusConfiguration : IEntityTypeConfiguration<AccountStatus>
{
    public void Configure(EntityTypeBuilder<AccountStatus> builder)
    {
        builder.ToTable("AccountStatus", "sec");
        builder.HasKey(x => x.AccountStatusId);

        builder.Property(x => x.AccountStatusId).HasColumnName("AccountStatusID");
        builder.Property(x => x.StatusName).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(1).IsFixedLength().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role", "sec");
        builder.HasKey(x => x.RoleId);

        builder.Property(x => x.RoleId).HasColumnName("RoleID");
        builder.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RoleDescription).HasMaxLength(255);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
    }
}

public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Module", "sec");
        builder.HasKey(x => x.ModuleId);

        builder.Property(x => x.ModuleId).HasColumnName("ModuleID");
        builder.Property(x => x.ModuleName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permission", "sec", table =>
        {
            table.HasCheckConstraint("CK_Permission_BitValue_Positive", "[BitValue] > 0");
            table.HasCheckConstraint("CK_Permission_BitValue_PowerOfTwo", "([BitValue] & ([BitValue] - 1)) = 0");
        });
        builder.HasKey(x => x.PermissionId);

        builder.Property(x => x.PermissionId).HasColumnName("PermissionID");
        builder.Property(x => x.PermissionName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole", "sec");
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.RoleId).HasColumnName("RoleID");
        builder.Property(x => x.AssignedBy).HasColumnName("AssignedBy");
        builder.Property(x => x.AssignedDate).HasColumnType("datetime2");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedByUser)
            .WithMany(x => x.AssignedUserRoles)
            .HasForeignKey(x => x.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "sec", table =>
        {
            table.HasCheckConstraint("CK_RolePermissions_PermissionMask_AllowedValues", "[PermissionMask] = -1 OR [PermissionMask] >= 0");
        });
        builder.HasKey(x => new { x.RoleId, x.ModuleId });

        builder.Property(x => x.RoleId).HasColumnName("RoleID");
        builder.Property(x => x.ModuleId).HasColumnName("ModuleID");

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions", "sec", table =>
        {
            table.HasCheckConstraint("CK_UserPermissions_PermissionMask_AllowedValues", "[PermissionMask] = -1 OR [PermissionMask] >= 0");
        });
        builder.HasKey(x => new { x.UserId, x.ModuleId });

        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.ModuleId).HasColumnName("ModuleID");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PermissionAuditConfiguration : IEntityTypeConfiguration<PermissionAudit>
{
    public void Configure(EntityTypeBuilder<PermissionAudit> builder)
    {
        builder.ToTable("PermissionAudit", "sec");
        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.AuditId).HasColumnName("AuditID");
        builder.Property(x => x.RoleId).HasColumnName("RoleID");
        builder.Property(x => x.ModuleId).HasColumnName("ModuleID");
        builder.Property(x => x.ChangedDate).HasColumnType("datetime2");

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken", "sec");
        builder.HasKey(x => x.RefreshTokenId);

        builder.Property(x => x.RefreshTokenId).HasColumnName("RefreshTokenID");
        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnType("datetime2");
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
        builder.Property(x => x.CreatedByIp).HasMaxLength(45);
        builder.Property(x => x.RevokedAt).HasColumnType("datetime2");
        builder.Property(x => x.RevokedByIp).HasMaxLength(45);
        builder.Property(x => x.ReplacedByTokenId).HasColumnName("ReplacedByTokenID");

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReplacedByToken)
            .WithMany()
            .HasForeignKey(x => x.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ActivityTypeConfiguration : IEntityTypeConfiguration<ActivityType>
{
    public void Configure(EntityTypeBuilder<ActivityType> builder)
    {
        builder.ToTable("ActivityType", "sec");
        builder.HasKey(x => x.ActivityTypeId);

        builder.Property(x => x.ActivityTypeId).HasColumnName("ActivityTypeID");
        builder.Property(x => x.ActivityTypeName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ActivityTypeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable("UserActivityLog", "sec");
        builder.HasKey(x => x.ActivityLogId);

        builder.Property(x => x.ActivityLogId).HasColumnName("ActivityLogID");
        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.ActivityTypeId).HasColumnName("ActivityTypeID");
        builder.Property(x => x.ActivityDetails).HasMaxLength(255);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.OccurredAt).HasColumnType("datetime2");

        builder.HasOne(x => x.User)
            .WithMany(x => x.ActivityLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActivityType)
            .WithMany(x => x.UserActivityLogs)
            .HasForeignKey(x => x.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
