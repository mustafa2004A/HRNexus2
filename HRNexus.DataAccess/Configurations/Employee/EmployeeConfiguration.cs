using HRNexus.DataAccess.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Configurations.Employee;

public sealed class EmploymentStatusConfiguration : IEntityTypeConfiguration<EmploymentStatus>
{
    public void Configure(EntityTypeBuilder<EmploymentStatus> builder)
    {
        builder.ToTable("EmploymentStatus", "emp");
        builder.HasKey(x => x.EmploymentStatusId);

        builder.Property(x => x.EmploymentStatusId).HasColumnName("EmploymentStatusID");
        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.EmploymentStatusCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class EmploymentTypeConfiguration : IEntityTypeConfiguration<EmploymentType>
{
    public void Configure(EntityTypeBuilder<EmploymentType> builder)
    {
        builder.ToTable("EmploymentType", "emp");
        builder.HasKey(x => x.EmploymentTypeId);

        builder.Property(x => x.EmploymentTypeId).HasColumnName("EmploymentTypeID");
        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.EmploymentTypeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Department", "emp");
        builder.HasKey(x => x.DepartmentId);

        builder.Property(x => x.DepartmentId).HasColumnName("DepartmentID");
        builder.Property(x => x.DepartmentName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DepartmentCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ParentDepartmentId).HasColumnName("ParentDepartmentID");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2");

        builder.HasOne(x => x.ParentDepartment)
            .WithMany(x => x.ChildDepartments)
            .HasForeignKey(x => x.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Position", "emp");
        builder.HasKey(x => x.PositionId);

        builder.Property(x => x.PositionId).HasColumnName("PositionID");
        builder.Property(x => x.PositionName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PositionCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2");
    }
}

public sealed class JobGradeConfiguration : IEntityTypeConfiguration<JobGrade>
{
    public void Configure(EntityTypeBuilder<JobGrade> builder)
    {
        builder.ToTable("JobGrade", "emp");
        builder.HasKey(x => x.JobGradeId);

        builder.Property(x => x.JobGradeId).HasColumnName("JobGradeID");
        builder.Property(x => x.GradeName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MinimumSalary).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MaximumSalary).HasColumnType("decimal(18,2)");
    }
}

public sealed class TerminationReasonConfiguration : IEntityTypeConfiguration<TerminationReason>
{
    public void Configure(EntityTypeBuilder<TerminationReason> builder)
    {
        builder.ToTable("TerminationReason", "emp");
        builder.HasKey(x => x.TerminationReasonId);

        builder.Property(x => x.TerminationReasonId).HasColumnName("TerminationReasonID");
        builder.Property(x => x.ReasonName).HasMaxLength(255).IsRequired();
    }
}

public sealed class RelationshipTypeConfiguration : IEntityTypeConfiguration<RelationshipType>
{
    public void Configure(EntityTypeBuilder<RelationshipType> builder)
    {
        builder.ToTable("RelationshipType", "emp");
        builder.HasKey(x => x.RelationshipTypeId);

        builder.Property(x => x.RelationshipTypeId).HasColumnName("RelationshipTypeID");
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RelationshipCode).HasMaxLength(10).IsRequired();
    }
}

public sealed class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("DocumentType", "emp");
        builder.HasKey(x => x.DocumentTypeId);

        builder.Property(x => x.DocumentTypeId).HasColumnName("DocumentTypeID");
        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
    }
}

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        builder.ToTable("Employee", "emp");
        builder.HasKey(x => x.EmployeeId);

        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.HireDate).HasColumnType("date");
        builder.Property(x => x.CurrentEmploymentStatusId).HasColumnName("CurrentEmploymentStatusID");
        builder.Property(x => x.TerminationReasonId).HasColumnName("TerminationReasonID");
        builder.Property(x => x.TerminationDate).HasColumnType("date");
        builder.Property(x => x.DeletedDate).HasColumnType("datetime2");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2");

        builder.HasOne(x => x.CurrentEmploymentStatus)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.CurrentEmploymentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TerminationReason)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.TerminationReasonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments", "emp");
        builder.HasKey(x => x.DocumentId);

        builder.Property(x => x.DocumentId).HasColumnName("DocumentID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.DocumentTypeId).HasColumnName("DocumentTypeID");
        builder.Property(x => x.FileStorageItemId).HasColumnName("FileStorageItemID");
        builder.Property(x => x.DocumentName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(50);
        builder.Property(x => x.FileExtension).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IssueDate).HasColumnType("date");
        builder.Property(x => x.ExpiryDate).HasColumnType("date");
        builder.Property(x => x.VerifiedDate).HasColumnType("datetime2");
        builder.Property(x => x.UploadedDate).HasColumnType("datetime2");
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeDocuments)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DocumentType)
            .WithMany(x => x.EmployeeDocuments)
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FileStorageItem)
            .WithMany(x => x.EmployeeDocuments)
            .HasForeignKey(x => x.FileStorageItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VerifiedByUser)
            .WithMany(x => x.VerifiedEmployeeDocuments)
            .HasForeignKey(x => x.VerifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedEmployeeDocuments)
            .HasForeignKey(x => x.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EmployeeJobHistoryConfiguration : IEntityTypeConfiguration<EmployeeJobHistory>
{
    public void Configure(EntityTypeBuilder<EmployeeJobHistory> builder)
    {
        builder.ToTable("EmployeeJobHistory", "emp");
        builder.HasKey(x => x.JobHistoryId);

        builder.Property(x => x.JobHistoryId).HasColumnName("JobHistoryID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.ManagerId).HasColumnName("ManagerID");
        builder.Property(x => x.DepartmentId).HasColumnName("DepartmentID");
        builder.Property(x => x.PositionId).HasColumnName("PositionID");
        builder.Property(x => x.EmploymentTypeId).HasColumnName("EmploymentTypeID");
        builder.Property(x => x.JobGradeId).HasColumnName("JobGradeID");
        builder.Property(x => x.EmploymentStatusId).HasColumnName("EmploymentStatusID");
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Position)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmploymentType)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.EmploymentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.JobGrade)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.JobGradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmploymentStatus)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.EmploymentStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EmployeeFamilyMemberConfiguration : IEntityTypeConfiguration<EmployeeFamilyMember>
{
    public void Configure(EntityTypeBuilder<EmployeeFamilyMember> builder)
    {
        builder.ToTable("EmployeeFamilyMember", "emp");
        builder.HasKey(x => x.FamilyMemberId);

        builder.Property(x => x.FamilyMemberId).HasColumnName("FamilyMemberID");
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.RelationshipTypeId).HasColumnName("RelationshipTypeID");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.FamilyMembers)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RelationshipType)
            .WithMany()
            .HasForeignKey(x => x.RelationshipTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
