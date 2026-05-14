using HRNexus.DataAccess.Entities.Leave;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRNexus.DataAccess.Configurations.Leave;

public sealed class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveType", "leave");
        builder.HasKey(x => x.LeaveTypeId);

        builder.Property(x => x.LeaveTypeId).HasColumnName("LeaveTypeID");
        builder.Property(x => x.LeaveTypeName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LeaveTypeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.DefaultDaysPerYear).HasColumnType("decimal(5,2)");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
    }
}

public sealed class RequestStatusConfiguration : IEntityTypeConfiguration<RequestStatus>
{
    public void Configure(EntityTypeBuilder<RequestStatus> builder)
    {
        builder.ToTable("RequestStatus", "leave");
        builder.HasKey(x => x.RequestStatusId);

        builder.Property(x => x.RequestStatusId).HasColumnName("RequestStatusID");
        builder.Property(x => x.StatusName).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StatusCode).HasMaxLength(1).IsFixedLength().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalance", "leave");
        builder.HasKey(x => x.LeaveBalanceId);

        builder.Property(x => x.LeaveBalanceId).HasColumnName("LeaveBalanceID");
        builder.Property(x => x.LeaveTypeId).HasColumnName("LeaveTypeID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.EntitledDays).HasColumnType("decimal(5,2)");
        builder.Property(x => x.UsedDays).HasColumnType("decimal(5,2)");
        builder.Property(x => x.RemainingDays).HasColumnType("decimal(5,2)");
        builder.Property(x => x.LastUpdated).HasColumnType("datetime2");

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveBalances)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequest", "leave");
        builder.HasKey(x => x.LeaveRequestId);

        builder.Property(x => x.LeaveRequestId).HasColumnName("LeaveRequestID");
        builder.Property(x => x.LeaveTypeId).HasColumnName("LeaveTypeID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.RequestedDays).HasColumnType("decimal(5,2)");
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.RequestStatusId).HasColumnName("RequestStatusID");
        builder.Property(x => x.RequestedAt).HasColumnType("datetime2");
        builder.Property(x => x.ReviewedBy).HasColumnName("ReviewedBy");
        builder.Property(x => x.ReviewedAt).HasColumnType("datetime2");
        builder.Property(x => x.ReviewNotes).HasMaxLength(500);

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestStatus)
            .WithMany(x => x.LeaveRequests)
            .HasForeignKey(x => x.RequestStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany(x => x.ReviewedLeaveRequests)
            .HasForeignKey(x => x.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holiday", "leave");
        builder.HasKey(x => x.HolidayId);

        builder.Property(x => x.HolidayId).HasColumnName("HolidayID");
        builder.Property(x => x.HolidayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.HolidayDate).HasColumnType("date");
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
    }
}

public sealed class LeaveAttachmentConfiguration : IEntityTypeConfiguration<LeaveAttachment>
{
    public void Configure(EntityTypeBuilder<LeaveAttachment> builder)
    {
        builder.ToTable("LeaveAttachment", "leave");
        builder.HasKey(x => x.LeaveAttachmentId);

        builder.Property(x => x.LeaveAttachmentId).HasColumnName("LeaveAttachmentID");
        builder.Property(x => x.LeaveRequestId).HasColumnName("LeaveRequestID");
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(10);
        builder.Property(x => x.FileStorageItemId).HasColumnName("FileStorageItemID");
        builder.Property(x => x.UploadedBy).HasColumnName("UploadedBy");
        builder.Property(x => x.UploadedAt).HasColumnType("datetime2");

        builder.HasOne(x => x.LeaveRequest)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedLeaveAttachments)
            .HasForeignKey(x => x.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FileStorageItem)
            .WithMany(x => x.LeaveAttachments)
            .HasForeignKey(x => x.FileStorageItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
