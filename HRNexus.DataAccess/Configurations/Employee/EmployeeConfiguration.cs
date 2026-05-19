using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Configurations.Employee;

public sealed class EmploymentStatusConfiguration : IEntityTypeConfiguration<EmploymentStatus>
{
    public void Configure(EntityTypeBuilder<EmploymentStatus> builder)
    {
        builder.ToTable("EmploymentStatus", "emp", table =>
        {
            table.HasCheckConstraint("CK_EmploymentStatus_EmploymentStatusCode_NotEmpty", "(len(ltrim(rtrim([EmploymentStatusCode])))>(0))");
            table.HasCheckConstraint("CK_EmploymentStatus_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.EmploymentStatusId).HasName("PK_EmploymentStatus");

        builder.Property(x => x.EmploymentStatusId).HasColumnName("EmploymentStatusID").UseIdentityColumn();
        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.EmploymentStatusCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.HasIndex(x => x.EmploymentStatusCode).IsUnique().HasDatabaseName("UQ_EmploymentStatus_EmploymentStatusCode");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_EmploymentStatus_Name");
    }
}

public sealed class EmploymentTypeConfiguration : IEntityTypeConfiguration<EmploymentType>
{
    public void Configure(EntityTypeBuilder<EmploymentType> builder)
    {
        builder.ToTable("EmploymentType", "emp", table =>
        {
            table.HasCheckConstraint("CK_EmploymentType_EmploymentTypeCode_NotEmpty", "(len(ltrim(rtrim([EmploymentTypeCode])))>(0))");
            table.HasCheckConstraint("CK_EmploymentType_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.EmploymentTypeId).HasName("PK_EmploymentType");

        builder.Property(x => x.EmploymentTypeId).HasColumnName("EmploymentTypeID").UseIdentityColumn();
        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.EmploymentTypeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.HasIndex(x => x.EmploymentTypeCode).IsUnique().HasDatabaseName("UQ_EmploymentType_EmploymentTypeCode");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_EmploymentType_Name");
    }
}

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Department", "emp", table =>
        {
            table.HasCheckConstraint("CK_Department_DepartmentCode_NotEmpty", "(len(ltrim(rtrim([DepartmentCode])))>(0))");
            table.HasCheckConstraint("CK_Department_DepartmentName_NotEmpty", "(len(ltrim(rtrim([DepartmentName])))>(0))");
            table.HasCheckConstraint("CK_Department_ModifiedDate_AfterCreatedDate", "([ModifiedDate] IS NULL OR [ModifiedDate]>=[CreatedDate])");
            table.HasCheckConstraint("CK_Department_ParentDepartmentID_NotSelf", "([ParentDepartmentID] IS NULL OR [ParentDepartmentID]<>[DepartmentID])");
        });
        builder.HasKey(x => x.DepartmentId).HasName("PK_Department");

        builder.Property(x => x.DepartmentId).HasColumnName("DepartmentID").UseIdentityColumn();
        builder.Property(x => x.DepartmentName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DepartmentCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ParentDepartmentId).HasColumnName("ParentDepartmentID");
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2(7)");

        builder.HasIndex(x => x.DepartmentCode).IsUnique().HasDatabaseName("UQ_Department_DepartmentCode");
        builder.HasIndex(x => x.DepartmentName).IsUnique().HasDatabaseName("UQ_Department_DepartmentName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_Department_IsActive");
        builder.HasIndex(x => x.ParentDepartmentId).HasDatabaseName("IX_Department_ParentDepartmentID");

        builder.HasOne(x => x.ParentDepartment)
            .WithMany(x => x.ChildDepartments)
            .HasForeignKey(x => x.ParentDepartmentId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Department_Department_ParentDepartmentID");
    }
}

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Position", "emp", table =>
        {
            table.HasCheckConstraint("CK_Position_ModifiedDate_AfterCreatedDate", "([ModifiedDate] IS NULL OR [ModifiedDate]>=[CreatedDate])");
            table.HasCheckConstraint("CK_Position_PositionCode_NotEmpty", "(len(ltrim(rtrim([PositionCode])))>(0))");
            table.HasCheckConstraint("CK_Position_PositionName_NotEmpty", "(len(ltrim(rtrim([PositionName])))>(0))");
        });
        builder.HasKey(x => x.PositionId).HasName("PK_Position");

        builder.Property(x => x.PositionId).HasColumnName("PositionID").UseIdentityColumn();
        builder.Property(x => x.PositionName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PositionCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2(7)");

        builder.HasIndex(x => x.PositionCode).IsUnique().HasDatabaseName("UQ_Position_PositionCode");
        builder.HasIndex(x => x.PositionName).IsUnique().HasDatabaseName("UQ_Position_PositionName");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_Position_IsActive");
    }
}

public sealed class JobGradeConfiguration : IEntityTypeConfiguration<JobGrade>
{
    public void Configure(EntityTypeBuilder<JobGrade> builder)
    {
        builder.ToTable("JobGrade", "emp", table =>
        {
            table.HasCheckConstraint("CK_JobGrade_GradeName_NotEmpty", "(len(ltrim(rtrim([GradeName])))>(0))");
            table.HasCheckConstraint("CK_JobGrade_MaximumSalary_Range", "([MaximumSalary]>=[MinimumSalary])");
            table.HasCheckConstraint("CK_JobGrade_MinimumSalary_NonNegative", "([MinimumSalary]>=(0))");
        });
        builder.HasKey(x => x.JobGradeId).HasName("PK_JobGrade");

        builder.Property(x => x.JobGradeId).HasColumnName("JobGradeID").UseIdentityColumn();
        builder.Property(x => x.GradeName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MinimumSalary).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MaximumSalary).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.GradeName).IsUnique().HasDatabaseName("UQ_JobGrade_GradeName");
    }
}

public sealed class TerminationReasonConfiguration : IEntityTypeConfiguration<TerminationReason>
{
    public void Configure(EntityTypeBuilder<TerminationReason> builder)
    {
        builder.ToTable("TerminationReason", "emp", table =>
        {
            table.HasCheckConstraint("CK_TerminationReason_ReasonName_NotEmpty", "(len(ltrim(rtrim([ReasonName])))>(0))");
        });
        builder.HasKey(x => x.TerminationReasonId).HasName("PK_TerminationReason");

        builder.Property(x => x.TerminationReasonId).HasColumnName("TerminationReasonID").UseIdentityColumn();
        builder.Property(x => x.ReasonName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IsEligibleForRehire).HasDefaultValue(false);

        builder.HasIndex(x => x.ReasonName).IsUnique().HasDatabaseName("UQ_TerminationReason_ReasonName");
    }
}

public sealed class RelationshipTypeConfiguration : IEntityTypeConfiguration<RelationshipType>
{
    public void Configure(EntityTypeBuilder<RelationshipType> builder)
    {
        builder.ToTable("RelationshipType", "emp", table =>
        {
            table.HasCheckConstraint("CK_RelationshipType_MaxEligibleAge_NonNegative", "([MaxEligibleAge] IS NULL OR [MaxEligibleAge]>=(0))");
            table.HasCheckConstraint("CK_RelationshipType_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
            table.HasCheckConstraint("CK_RelationshipType_RelationshipCode_NotEmpty", "(len(ltrim(rtrim([RelationshipCode])))>(0))");
        });
        builder.HasKey(x => x.RelationshipTypeId).HasName("PK_RelationshipType");

        builder.Property(x => x.RelationshipTypeId).HasColumnName("RelationshipTypeID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RelationshipCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsEligibleForVisa).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_RelationshipType_Name");
        builder.HasIndex(x => x.RelationshipCode).IsUnique().HasDatabaseName("UQ_RelationshipType_RelationshipCode");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_RelationshipType_IsActive");
    }
}

public sealed class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("DocumentType", "emp", table =>
        {
            table.HasCheckConstraint("CK_DocumentType_Code_NotEmpty", "(len(ltrim(rtrim([Code])))>(0))");
            table.HasCheckConstraint("CK_DocumentType_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.DocumentTypeId).HasName("PK_DocumentType");

        builder.Property(x => x.DocumentTypeId).HasColumnName("DocumentTypeID").UseIdentityColumn();
        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsExpiryTracked).HasDefaultValue(false);
        builder.Property(x => x.IsMandatory).HasDefaultValue(false);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_DocumentType_Code");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_DocumentType_Name");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_DocumentType_IsActive");
    }
}

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        builder.ToTable("Employee", "emp", table =>
        {
            table.HasCheckConstraint("CK_Employee_DeleteState", "([IsDeleted]=(0) AND [DeletedBy] IS NULL AND [DeletedDate] IS NULL OR [IsDeleted]=(1))");
            table.HasCheckConstraint("CK_Employee_EmployeeCode_NotEmpty", "(len(ltrim(rtrim([EmployeeCode])))>(0))");
            table.HasCheckConstraint("CK_Employee_ModifiedDate_AfterCreatedDate", "([ModifiedDate] IS NULL OR [ModifiedDate]>=[CreatedDate])");
            table.HasCheckConstraint("CK_Employee_TerminationDate_Range", "([TerminationDate] IS NULL OR [TerminationDate]>=[HireDate])");
            table.HasCheckConstraint("CK_Employee_TerminationState", "([TerminationReasonID] IS NULL AND [TerminationDate] IS NULL OR [TerminationReasonID] IS NOT NULL AND [TerminationDate] IS NOT NULL)");
        });
        builder.HasKey(x => x.EmployeeId).HasName("PK_Employee");

        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID").UseIdentityColumn();
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.HireDate).HasColumnType("date");
        builder.Property(x => x.CurrentEmploymentStatusId).HasColumnName("CurrentEmploymentStatusID");
        builder.Property(x => x.TerminationReasonId).HasColumnName("TerminationReasonID");
        builder.Property(x => x.TerminationDate).HasColumnType("date");
        builder.Property(x => x.IsEligibleForRehire).HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedBy).HasColumnName("DeletedBy");
        builder.Property(x => x.DeletedDate).HasColumnType("datetime2(7)");
        builder.Property(x => x.CreatedBy).HasColumnName("CreatedBy");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2(7)");

        builder.HasIndex(x => x.EmployeeCode).IsUnique().HasDatabaseName("UQ_Employee_EmployeeCode");
        builder.HasIndex(x => x.PersonId).IsUnique().HasDatabaseName("UQ_Employee_PersonID");
        builder.HasIndex(x => x.CreatedBy).HasDatabaseName("IX_Employee_CreatedBy");
        builder.HasIndex(x => x.CurrentEmploymentStatusId).HasDatabaseName("IX_Employee_CurrentEmploymentStatusID");
        builder.HasIndex(x => x.DeletedBy).HasDatabaseName("IX_Employee_DeletedBy");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Employee_IsDeleted");
        builder.HasIndex(x => x.ModifiedBy).HasDatabaseName("IX_Employee_ModifiedBy");
        builder.HasIndex(x => x.TerminationReasonId).HasDatabaseName("IX_Employee_TerminationReasonID");

        builder.HasOne(x => x.CurrentEmploymentStatus)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.CurrentEmploymentStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Employee_EmploymentStatus_CurrentEmploymentStatusID");

        builder.HasOne(x => x.TerminationReason)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.TerminationReasonId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Employee_TerminationReason_TerminationReasonID");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Employee_User_CreatedBy");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.DeletedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Employee_User_DeletedBy");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Employee_User_ModifiedBy");
    }
}

public sealed class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments", "emp", table =>
        {
            table.HasCheckConstraint("CK_EmployeeDocuments_DateRange", "([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate])");
            table.HasCheckConstraint("CK_EmployeeDocuments_DocumentName_NotEmpty", "(len(ltrim(rtrim([DocumentName])))>(0))");
            table.HasCheckConstraint("CK_EmployeeDocuments_FileExtension_NotEmpty", "(len(ltrim(rtrim([FileExtension])))>(0))");
            table.HasCheckConstraint("CK_EmployeeDocuments_FilePath_NotEmpty", "(len(ltrim(rtrim([FilePath])))>(0))");
            table.HasCheckConstraint("CK_EmployeeDocuments_Remarks_NotEmpty", "([Remarks] IS NULL OR len(ltrim(rtrim([Remarks])))>(0))");
            table.HasCheckConstraint("CK_EmployeeDocuments_VerificationState", "([IsVerified]=(0) AND [VerifiedBy] IS NULL AND [VerifiedDate] IS NULL OR [IsVerified]=(1) AND [VerifiedDate] IS NOT NULL)");
        });
        builder.HasKey(x => x.DocumentId).HasName("PK_EmployeeDocuments");

        builder.Property(x => x.DocumentId).HasColumnName("DocumentID").UseIdentityColumn();
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.DocumentTypeId).HasColumnName("DocumentTypeID");
        builder.Property(x => x.FileStorageItemId).HasColumnName("FileStorageItemID");
        builder.Property(x => x.DocumentName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(50);
        builder.Property(x => x.FilePath).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(10).IsRequired();
        builder.Property(x => x.IssueDate).HasColumnType("date");
        builder.Property(x => x.ExpiryDate).HasColumnType("date");
        builder.Property(x => x.IsVerified).HasDefaultValue(false);
        builder.Property(x => x.VerifiedBy).HasColumnName("VerifiedBy");
        builder.Property(x => x.VerifiedDate).HasColumnType("datetime2(7)");
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.UploadedBy).HasColumnName("UploadedBy");
        builder.Property(x => x.UploadedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.DocumentTypeId).HasDatabaseName("IX_EmployeeDocuments_DocumentTypeID");
        builder.HasIndex(x => new { x.EmployeeId, x.DocumentTypeId }).HasDatabaseName("IX_EmployeeDocuments_EmployeeID_DocumentTypeID");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_EmployeeDocuments_IsActive");
        builder.HasIndex(x => x.ReferenceNumber).HasDatabaseName("IX_EmployeeDocuments_ReferenceNumber");
        builder.HasIndex(x => x.UploadedBy).HasDatabaseName("IX_EmployeeDocuments_UploadedBy");
        builder.HasIndex(x => x.VerifiedBy).HasDatabaseName("IX_EmployeeDocuments_VerifiedBy");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeDocuments)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeDocuments_Employee");

        builder.HasOne(x => x.DocumentType)
            .WithMany(x => x.EmployeeDocuments)
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeDocuments_DocumentType");

        builder.HasOne(x => x.FileStorageItem)
            .WithMany(x => x.EmployeeDocuments)
            .HasForeignKey(x => x.FileStorageItemId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeDocuments_FileStorageItem_FileStorageItemID");

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedEmployeeDocuments)
            .HasForeignKey(x => x.UploadedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeDocuments_User_UploadedBy");

        builder.HasOne(x => x.VerifiedByUser)
            .WithMany(x => x.VerifiedEmployeeDocuments)
            .HasForeignKey(x => x.VerifiedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeDocuments_User_VerifiedBy");
    }
}

public sealed class EmployeeJobHistoryConfiguration : IEntityTypeConfiguration<EmployeeJobHistory>
{
    public void Configure(EntityTypeBuilder<EmployeeJobHistory> builder)
    {
        builder.ToTable("EmployeeJobHistory", "emp", table =>
        {
            table.HasCheckConstraint("CK_EmployeeJobHistory_DateRange", "([EndDate] IS NULL OR [EndDate]>=[StartDate])");
            table.HasCheckConstraint("CK_EmployeeJobHistory_IsCurrent_EndDate", "([IsCurrent]=(0) OR [EndDate] IS NULL)");
            table.HasCheckConstraint("CK_EmployeeJobHistory_ManagerID_NotSelf", "([ManagerID] IS NULL OR [ManagerID]<>[EmployeeID])");
        });
        builder.HasKey(x => x.JobHistoryId).HasName("PK_EmployeeJobHistory");

        builder.Property(x => x.JobHistoryId).HasColumnName("JobHistoryID").UseIdentityColumn();
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.ManagerId).HasColumnName("ManagerID");
        builder.Property(x => x.DepartmentId).HasColumnName("DepartmentID");
        builder.Property(x => x.PositionId).HasColumnName("PositionID");
        builder.Property(x => x.EmploymentTypeId).HasColumnName("EmploymentTypeID");
        builder.Property(x => x.JobGradeId).HasColumnName("JobGradeID");
        builder.Property(x => x.EmploymentStatusId).HasColumnName("EmploymentStatusID");
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.IsCurrent).HasDefaultValue(false);

        builder.HasIndex(x => x.DepartmentId).HasDatabaseName("IX_EmployeeJobHistory_DepartmentID");

        builder.HasIndex(x => x.EmployeeId)
            .IsUnique()
            .HasFilter("([IsCurrent]=(1))")
            .HasDatabaseName("IX_EmployeeJobHistory_EmployeeID_IsCurrent");

        builder.HasIndex(x => new { x.EmployeeId, x.StartDate }).HasDatabaseName("IX_EmployeeJobHistory_EmployeeID_StartDate");
        builder.HasIndex(x => x.EmploymentStatusId).HasDatabaseName("IX_EmployeeJobHistory_EmploymentStatusID");
        builder.HasIndex(x => x.EmploymentTypeId).HasDatabaseName("IX_EmployeeJobHistory_EmploymentTypeID");
        builder.HasIndex(x => x.JobGradeId).HasDatabaseName("IX_EmployeeJobHistory_JobGradeID");
        builder.HasIndex(x => x.ManagerId).HasDatabaseName("IX_EmployeeJobHistory_ManagerID");
        builder.HasIndex(x => x.PositionId).HasDatabaseName("IX_EmployeeJobHistory_PositionID");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeJobHistory_Employee_EmployeeID");

        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeJobHistory_Employee_ManagerID");

        builder.HasOne(x => x.Department)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeJobHistory_Department");

        builder.HasOne(x => x.Position)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeJobHistory_Position");

        builder.HasOne(x => x.EmploymentType)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.EmploymentTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeJobHistory_EmploymentType");

        builder.HasOne(x => x.JobGrade)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.JobGradeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeJobHistory_JobGrade");

        builder.HasOne(x => x.EmploymentStatus)
            .WithMany(x => x.JobHistories)
            .HasForeignKey(x => x.EmploymentStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeJobHistory_EmploymentStatus");
    }
}

public sealed class EmployeeFamilyMemberConfiguration : IEntityTypeConfiguration<EmployeeFamilyMember>
{
    public void Configure(EntityTypeBuilder<EmployeeFamilyMember> builder)
    {
        builder.ToTable("EmployeeFamilyMember", "emp");
        builder.HasKey(x => x.FamilyMemberId).HasName("PK_EmployeeFamilyMember");

        builder.Property(x => x.FamilyMemberId).HasColumnName("FamilyMemberID").UseIdentityColumn();
        builder.Property(x => x.EmployeeId).HasColumnName("EmployeeID");
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.RelationshipTypeId).HasColumnName("RelationshipTypeID");

        builder.HasIndex(x => new { x.EmployeeId, x.PersonId }).IsUnique().HasDatabaseName("UQ_EmployeeFamilyMember_EmployeeID_PersonID");
        builder.HasIndex(x => x.PersonId).HasDatabaseName("IX_EmployeeFamilyMember_PersonID");
        builder.HasIndex(x => x.RelationshipTypeId).HasDatabaseName("IX_EmployeeFamilyMember_RelationshipTypeID");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.FamilyMembers)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeFamilyMember_Employee");

        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeFamilyMember_Person");

        builder.HasOne(x => x.RelationshipType)
            .WithMany()
            .HasForeignKey(x => x.RelationshipTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_EmployeeFamilyMember_RelationshipType");
    }
}
