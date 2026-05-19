using HRNexus.DataAccess.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRNexus.DataAccess.Configurations.Security;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", "sec", table =>
        {
            table.HasCheckConstraint("CK_User_FailedLoginAttempts_NonNegative", "([FailedLoginAttempts]>=(0))");
            table.HasCheckConstraint("CK_User_ModifiedDate_AfterCreatedDate", "([ModifiedDate] IS NULL OR [ModifiedDate]>=[CreatedDate])");
            table.HasCheckConstraint("CK_User_PasswordHash_NotEmpty", "(len(ltrim(rtrim([PasswordHash])))>(0))");
            table.HasCheckConstraint("CK_User_Username_NotEmpty", "(len(ltrim(rtrim([Username])))>(0))");
        });
        builder.HasKey(x => x.UserId).HasName("PK_User");

        builder.Property(x => x.UserId).HasColumnName("UserID").UseIdentityColumn();
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.LastLoginAt).HasColumnType("datetime2(7)");
        builder.Property(x => x.FailedLoginAttempts).HasDefaultValue(0);
        builder.Property(x => x.AccountStatusId).HasColumnName("AccountStatusID");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2(7)");

        builder.HasIndex(x => x.Username).IsUnique().HasDatabaseName("UQ_User_Username");
        builder.HasIndex(x => x.AccountStatusId).HasDatabaseName("IX_User_AccountStatusID");

        builder.HasIndex(x => x.EmployeeId)
            .IsUnique()
            .HasFilter("([EmployeeID] IS NOT NULL)")
            .HasDatabaseName("IX_User_EmployeeID");

        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_User_IsActive");

        builder.HasOne(x => x.Employee)
            .WithOne(x => x.User)
            .HasForeignKey<User>(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_User_Employee");

        builder.HasOne(x => x.AccountStatus)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.AccountStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_User_AccountStatus");
    }
}

public sealed class AccountStatusConfiguration : IEntityTypeConfiguration<AccountStatus>
{
    public void Configure(EntityTypeBuilder<AccountStatus> builder)
    {
        builder.ToTable("AccountStatus", "sec", table =>
        {
            table.HasCheckConstraint("CK_AccountStatus_StatusCode_Len", "(len([StatusCode])=(1))");
            table.HasCheckConstraint("CK_AccountStatus_StatusName_NotEmpty", "(len(ltrim(rtrim([StatusName])))>(0))");
        });
        builder.HasKey(x => x.AccountStatusId).HasName("PK_AccountStatus");

        builder.Property(x => x.AccountStatusId).HasColumnName("AccountStatusID").UseIdentityColumn();
        builder.Property(x => x.StatusName).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StatusCode).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.StatusCode).IsUnique().HasDatabaseName("UQ_AccountStatus_StatusCode");
        builder.HasIndex(x => x.StatusName).IsUnique().HasDatabaseName("UQ_AccountStatus_StatusName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_AccountStatus_IsActive");
    }
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role", "sec", table =>
        {
            table.HasCheckConstraint("CK_Role_RoleName_NotEmpty", "(len(ltrim(rtrim([RoleName])))>(0))");
        });
        builder.HasKey(x => x.RoleId).HasName("PK_Role");

        builder.Property(x => x.RoleId).HasColumnName("RoleID").UseIdentityColumn();
        builder.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RoleDescription).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => x.RoleName).IsUnique().HasDatabaseName("UQ_Role_RoleName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_Role_IsActive");
    }
}

public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Module", "sec", table =>
        {
            table.HasCheckConstraint("CK_Module_ModuleName_NotEmpty", "(len(ltrim(rtrim([ModuleName])))>(0))");
        });
        builder.HasKey(x => x.ModuleId).HasName("PK_Module");

        builder.Property(x => x.ModuleId).HasColumnName("ModuleID").UseIdentityColumn();
        builder.Property(x => x.ModuleName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.ModuleName).IsUnique().HasDatabaseName("UQ_Module_ModuleName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_Module_IsActive");
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permission", "sec", table =>
        {
            table.HasCheckConstraint("CK_Permission_BitOrder_NonNegative", "([BitOrder]>=(0))");
            table.HasCheckConstraint("CK_Permission_BitValue_Positive", "([BitValue]>(0))");
            table.HasCheckConstraint("CK_Permission_BitValue_PowerOfTwo", "(([BitValue]&([BitValue]-(1)))=(0))");
            table.HasCheckConstraint("CK_Permission_PermissionName_NotEmpty", "(len(ltrim(rtrim([PermissionName])))>(0))");
        });
        builder.HasKey(x => x.PermissionId).HasName("PK_Permission");

        builder.Property(x => x.PermissionId).HasColumnName("PermissionID").UseIdentityColumn();
        builder.Property(x => x.PermissionName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BitValue);
        builder.Property(x => x.BitOrder);
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsSystem).HasDefaultValue(true);

        builder.HasIndex(x => x.BitOrder).IsUnique().HasDatabaseName("UQ_Permission_BitOrder");
        builder.HasIndex(x => x.BitValue).IsUnique().HasDatabaseName("UQ_Permission_BitValue");
        builder.HasIndex(x => x.PermissionName).IsUnique().HasDatabaseName("UQ_Permission_PermissionName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_Permission_IsActive");
        builder.HasIndex(x => x.IsSystem).HasDatabaseName("IX_Permission_IsSystem");
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole", "sec");
        builder.HasKey(x => new { x.UserId, x.RoleId }).HasName("PK_UserRole");

        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.RoleId).HasColumnName("RoleID");
        builder.Property(x => x.AssignedBy).HasColumnName("AssignedBy");
        builder.Property(x => x.AssignedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => x.AssignedBy).HasDatabaseName("IX_UserRole_AssignedBy");
        builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_UserRole_RoleID");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserRole_User");

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserRole_Role");

        builder.HasOne(x => x.AssignedByUser)
            .WithMany(x => x.AssignedUserRoles)
            .HasForeignKey(x => x.AssignedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserRole_User_AssignedBy");
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "sec", table =>
        {
            table.HasCheckConstraint("CK_RolePermissions_PermissionMask_AllowedValues", "([PermissionMask]=(-1) OR [PermissionMask]>=(0))");
        });
        builder.HasKey(x => new { x.RoleId, x.ModuleId }).HasName("PK_RolePermissions");

        builder.Property(x => x.RoleId).HasColumnName("RoleID");
        builder.Property(x => x.ModuleId).HasColumnName("ModuleID");
        builder.Property(x => x.PermissionMask).HasDefaultValue(0);

        builder.HasIndex(x => x.ModuleId).HasDatabaseName("IX_RolePermissions_ModuleID");

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_RolePermissions_Role");

        builder.HasOne(x => x.Module)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_RolePermissions_Module");
    }
}

public sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions", "sec", table =>
        {
            table.HasCheckConstraint("CK_UserPermissions_PermissionMask_AllowedValues", "([PermissionMask]=(-1) OR [PermissionMask]>=(0))");
        });
        builder.HasKey(x => new { x.UserId, x.ModuleId }).HasName("PK_UserPermissions");

        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.ModuleId).HasColumnName("ModuleID");
        builder.Property(x => x.PermissionMask).HasDefaultValue(0);

        builder.HasIndex(x => x.ModuleId).HasDatabaseName("IX_UserPermissions_ModuleID");

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserPermissions_User");

        builder.HasOne(x => x.Module)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserPermissions_Module");
    }
}

public sealed class PermissionAuditConfiguration : IEntityTypeConfiguration<PermissionAudit>
{
    public void Configure(EntityTypeBuilder<PermissionAudit> builder)
    {
        builder.ToTable("PermissionAudit", "sec", table =>
        {
            table.HasCheckConstraint("CK_PermissionAudit_Masks_AllowedValues", "(([OldMask]=(-1) OR [OldMask]>=(0)) AND ([NewMask]=(-1) OR [NewMask]>=(0)))");
        });
        builder.HasKey(x => x.AuditId).HasName("PK_PermissionAudit");

        builder.Property(x => x.AuditId).HasColumnName("AuditID").UseIdentityColumn();
        builder.Property(x => x.RoleId).HasColumnName("RoleID");
        builder.Property(x => x.ModuleId).HasColumnName("ModuleID");
        builder.Property(x => x.ChangedBy).HasColumnName("ChangedBy");
        builder.Property(x => x.ChangedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => x.ChangedBy).HasDatabaseName("IX_PermissionAudit_ChangedBy");
        builder.HasIndex(x => x.ChangedDate).HasDatabaseName("IX_PermissionAudit_ChangedDate");
        builder.HasIndex(x => x.ModuleId).HasDatabaseName("IX_PermissionAudit_ModuleID");
        builder.HasIndex(x => new { x.RoleId, x.ModuleId }).HasDatabaseName("IX_PermissionAudit_RoleID_ModuleID");

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PermissionAudit_Role");

        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PermissionAudit_Module");

        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PermissionAudit_User_ChangedBy");
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken", "sec", table =>
        {
            table.HasCheckConstraint("CK_RefreshToken_ExpiryAfterCreate", "([ExpiresAt]>[CreatedAt])");
            table.HasCheckConstraint("CK_RefreshToken_RevokedAt_AfterCreate", "([RevokedAt] IS NULL OR [RevokedAt]>=[CreatedAt])");
        });
        builder.HasKey(x => x.RefreshTokenId).HasName("PK_RefreshToken");

        builder.Property(x => x.RefreshTokenId).HasColumnName("RefreshTokenID").UseIdentityColumn();
        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnType("datetime2(7)");
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.CreatedByIp).HasMaxLength(45);
        builder.Property(x => x.RevokedAt).HasColumnType("datetime2(7)");
        builder.Property(x => x.RevokedByIp).HasMaxLength(45);
        builder.Property(x => x.ReplacedByTokenId).HasColumnName("ReplacedByTokenID");

        builder.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("UQ_RefreshToken_TokenHash");
        builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("IX_RefreshToken_ExpiresAt");
        builder.HasIndex(x => x.RevokedAt).HasDatabaseName("IX_RefreshToken_RevokedAt");
        builder.HasIndex(x => x.UserId).HasDatabaseName("IX_RefreshToken_UserID");
        builder.HasIndex(x => new { x.UserId, x.RevokedAt }).HasDatabaseName("IX_RefreshToken_UserID_RevokedAt");

        builder.HasIndex(x => x.ReplacedByTokenId)
            .IsUnique()
            .HasFilter("([ReplacedByTokenID] IS NOT NULL)")
            .HasDatabaseName("UX_RefreshToken_ReplacedByTokenID");

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_RefreshToken_User");

        builder.HasOne(x => x.ReplacedByToken)
            .WithMany()
            .HasForeignKey(x => x.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_RefreshToken_ReplacedByToken");
    }
}

public sealed class ActivityTypeConfiguration : IEntityTypeConfiguration<ActivityType>
{
    public void Configure(EntityTypeBuilder<ActivityType> builder)
    {
        builder.ToTable("ActivityType", "sec", table =>
        {
            table.HasCheckConstraint("CK_ActivityType_ActivityTypeCode_NotEmpty", "(len(ltrim(rtrim([ActivityTypeCode])))>(0))");
            table.HasCheckConstraint("CK_ActivityType_ActivityTypeName_NotEmpty", "(len(ltrim(rtrim([ActivityTypeName])))>(0))");
        });
        builder.HasKey(x => x.ActivityTypeId).HasName("PK_ActivityType");

        builder.Property(x => x.ActivityTypeId).HasColumnName("ActivityTypeID").UseIdentityColumn();
        builder.Property(x => x.ActivityTypeName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ActivityTypeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.ActivityTypeCode).IsUnique().HasDatabaseName("UQ_ActivityType_ActivityTypeCode");
        builder.HasIndex(x => x.ActivityTypeName).IsUnique().HasDatabaseName("UQ_ActivityType_ActivityTypeName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_ActivityType_IsActive");
    }
}

public sealed class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable("UserActivityLog", "sec");
        builder.HasKey(x => x.ActivityLogId).HasName("PK_UserActivityLog");

        builder.Property(x => x.ActivityLogId).HasColumnName("ActivityLogID").UseIdentityColumn();
        builder.Property(x => x.UserId).HasColumnName("UserID");
        builder.Property(x => x.ActivityTypeId).HasColumnName("ActivityTypeID");
        builder.Property(x => x.ActivityDetails).HasMaxLength(255);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.OccurredAt).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => x.ActivityTypeId).HasDatabaseName("IX_UserActivityLog_ActivityTypeID");
        builder.HasIndex(x => x.IsSuccess).HasDatabaseName("IX_UserActivityLog_IsSuccess");
        builder.HasIndex(x => x.OccurredAt).HasDatabaseName("IX_UserActivityLog_OccurredAt");
        builder.HasIndex(x => x.UserId).HasDatabaseName("IX_UserActivityLog_UserID");

        builder.HasOne(x => x.User)
            .WithMany(x => x.ActivityLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserActivityLog_User");

        builder.HasOne(x => x.ActivityType)
            .WithMany(x => x.UserActivityLogs)
            .HasForeignKey(x => x.ActivityTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserActivityLog_ActivityType");
    }
}
