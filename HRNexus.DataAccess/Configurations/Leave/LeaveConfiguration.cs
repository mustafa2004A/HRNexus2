using HRNexus.DataAccess.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRNexus.DataAccess.Configurations.Leave;

public sealed class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveType", "leave", table =>
        {
            table.HasCheckConstraint("CK_LeaveType_DefaultDaysPerYear_NonNegative", "([DefaultDaysPerYear]>=(0))");
            table.HasCheckConstraint("CK_LeaveType_LeaveTypeCode_NotEmpty", "(len(ltrim(rtrim([LeaveTypeCode])))>(0))");
            table.HasCheckConstraint("CK_LeaveType_LeaveTypeName_NotEmpty", "(len(ltrim(rtrim([LeaveTypeName])))>(0))");
        });
        builder.HasKey(x => x.LeaveTypeId).HasName("PK_LeaveType");

        builder.Property(x => x.LeaveTypeId).HasColumnName("LeaveTypeID").UseIdentityColumn();
        builder.Property(x => x.LeaveTypeName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LeaveTypeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.DefaultDaysPerYear).HasColumnType("decimal(5,2)");
        builder.Property(x => x.IsPaid).HasDefaultValue(true);
        builder.Property(x => x.RequiresApproval).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => x.LeaveTypeCode).IsUnique().HasDatabaseName("UQ_LeaveType_LeaveTypeCode");
        builder.HasIndex(x => x.LeaveTypeName).IsUnique().HasDatabaseName("UQ_LeaveType_LeaveTypeName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_LeaveType_IsActive");
    }
}

public sealed class RequestStatusConfiguration : IEntityTypeConfiguration<RequestStatus>
{
    public void Configure(EntityTypeBuilder<RequestStatus> builder)
    {
        builder.ToTable("RequestStatus", "leave", table =>
        {
            table.HasCheckConstraint("CK_RequestStatus_StatusCode_Len", "(len(ltrim(rtrim([StatusCode])))=(1))");
            table.HasCheckConstraint("CK_RequestStatus_StatusName_NotEmpty", "(len(ltrim(rtrim([StatusName])))>(0))");
        });
        builder.HasKey(x => x.RequestStatusId).HasName("PK_RequestStatus");

        builder.Property(x => x.RequestStatusId).HasColumnName("RequestStatusID").UseIdentityColumn();
        builder.Property(x => x.StatusName).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StatusCode).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsFinalState).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.StatusCode).IsUnique().HasDatabaseName("UQ_RequestStatus_StatusCode");
        builder.HasIndex(x => x.StatusName).IsUnique().HasDatabaseName("UQ_RequestStatus_StatusName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_RequestStatus_IsActive");
    }
}

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalance", "leave", table =>
        {
            table.HasCheckConstraint("CK_LeaveBalance_BalanceYear_Range", "([BalanceYear]>=(2000) AND [BalanceYear]<=(2100))");
            table.HasCheckConstraint("CK_LeaveBalance_EntitledDays_NonNegative", "([EntitledDays]>=(0))");
            table.HasCheckConstraint("CK_LeaveBalance_RemainingDays_NonNegative", "([RemainingDays]>=(0))");
            table.HasCheckConstraint("CK_LeaveBalance_UsedDays_NonNegative", "([UsedDays]>=(0))");
        });
        builder.HasKey(x => x.LeaveBalanceId).HasName("PK_LeaveBalance");

        builder.Property(x => x.LeaveBalanceId).HasColumnName("LeaveBalanceID").UseIdentityColumn();
        builder.Property(x => x.LeaveTypeId).HasColumnName("LeaveTypeID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.EntitledDays).HasColumnType("decimal(5,2)");
        builder.Property(x => x.UsedDays).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
        builder.Property(x => x.RemainingDays).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
        builder.Property(x => x.LastUpdated).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.BalanceYear })
            .IsUnique()
            .HasDatabaseName("UQ_LeaveBalance_EmployeeID_LeaveTypeID_BalanceYear");

        builder.HasIndex(x => new { x.EmployeeId, x.BalanceYear }).HasDatabaseName("IX_LeaveBalance_EmployeeID_BalanceYear");
        builder.HasIndex(x => x.LeaveTypeId).HasDatabaseName("IX_LeaveBalance_LeaveTypeID");

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveBalance_LeaveType");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveBalance_Employee");
    }
}

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequest", "leave", table =>
        {
            table.HasCheckConstraint("CK_LeaveRequest_DateRange", "([EndDate]>=[StartDate])");
            table.HasCheckConstraint("CK_LeaveRequest_RequestedDays_Positive", "([RequestedDays]>(0))");
            table.HasCheckConstraint("CK_LeaveRequest_ReviewedAt_AfterRequestedAt", "([ReviewedAt] IS NULL OR [ReviewedAt]>=[RequestedAt])");
        });
        builder.HasKey(x => x.LeaveRequestId).HasName("PK_LeaveRequest");

        builder.Property(x => x.LeaveRequestId).HasColumnName("LeaveRequestID").UseIdentityColumn();
        builder.Property(x => x.LeaveTypeId).HasColumnName("LeaveTypeID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.RequestedDays).HasColumnType("decimal(5,2)");
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.RequestStatusId).HasColumnName("RequestStatusID");
        builder.Property(x => x.RequestedAt).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.ReviewedBy).HasColumnName("ReviewedBy");
        builder.Property(x => x.ReviewedAt).HasColumnType("datetime2(7)");
        builder.Property(x => x.ReviewNotes).HasMaxLength(500);

        builder.HasIndex(x => new { x.EmployeeId, x.StartDate }).HasDatabaseName("IX_LeaveRequest_EmployeeID_StartDate");
        builder.HasIndex(x => x.LeaveTypeId).HasDatabaseName("IX_LeaveRequest_LeaveTypeID");
        builder.HasIndex(x => x.RequestStatusId).HasDatabaseName("IX_LeaveRequest_RequestStatusID");
        builder.HasIndex(x => x.ReviewedBy).HasDatabaseName("IX_LeaveRequest_ReviewedBy");

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveRequest_LeaveType");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveRequest_Employee");

        builder.HasOne(x => x.RequestStatus)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.RequestStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveRequest_RequestStatus");

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany(x => x.ReviewedLeaveRequests)
            .HasForeignKey(x => x.ReviewedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveRequest_User_ReviewedBy");
    }
}

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holiday", "leave", table =>
        {
            table.HasCheckConstraint("CK_Holiday_HolidayName_NotEmpty", "(len(ltrim(rtrim([HolidayName])))>(0))");
        });
        builder.HasKey(x => x.HolidayId).HasName("PK_Holiday");

        builder.Property(x => x.HolidayId).HasColumnName("HolidayID").UseIdentityColumn();
        builder.Property(x => x.HolidayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.HolidayDate).HasColumnType("date");
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsRecurringAnnual).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => new { x.HolidayDate, x.HolidayName })
            .IsUnique()
            .HasDatabaseName("UQ_Holiday_HolidayDate_HolidayName");

        builder.HasIndex(x => x.HolidayDate).HasDatabaseName("IX_Holiday_HolidayDate");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_Holiday_IsActive");
    }
}

public sealed class LeaveAttachmentConfiguration : IEntityTypeConfiguration<LeaveAttachment>
{
    public void Configure(EntityTypeBuilder<LeaveAttachment> builder)
    {
        builder.ToTable("LeaveAttachment", "leave", table =>
        {
            table.HasCheckConstraint("CK_LeaveAttachment_FileName_NotEmpty", "(len(ltrim(rtrim([FileName])))>(0))");
            table.HasCheckConstraint("CK_LeaveAttachment_FilePath_NotEmpty", "(len(ltrim(rtrim([FilePath])))>(0))");
        });
        builder.HasKey(x => x.LeaveAttachmentId).HasName("PK_LeaveAttachment");

        builder.Property(x => x.LeaveAttachmentId).HasColumnName("LeaveAttachmentID").UseIdentityColumn();
        builder.Property(x => x.LeaveRequestId).HasColumnName("LeaveRequestID");
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FilePath).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(10);
        builder.Property(x => x.FileStorageItemId).HasColumnName("FileStorageItemID");
        builder.Property(x => x.UploadedBy).HasColumnName("UploadedBy");
        builder.Property(x => x.UploadedAt).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_LeaveAttachment_IsActive");
        builder.HasIndex(x => x.LeaveRequestId).HasDatabaseName("IX_LeaveAttachment_LeaveRequestID");
        builder.HasIndex(x => x.UploadedBy).HasDatabaseName("IX_LeaveAttachment_UploadedBy");

        builder.HasOne(x => x.LeaveRequest)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.LeaveRequestId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveAttachment_LeaveRequest");

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedLeaveAttachments)
            .HasForeignKey(x => x.UploadedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveAttachment_User_UploadedBy");

        builder.HasOne(x => x.FileStorageItem)
            .WithMany(x => x.LeaveAttachments)
            .HasForeignKey(x => x.FileStorageItemId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_LeaveAttachment_FileStorageItem_FileStorageItemID");
    }
}
