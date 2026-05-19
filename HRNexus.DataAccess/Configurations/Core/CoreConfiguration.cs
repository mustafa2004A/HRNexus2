using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Configurations.Core;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Country", "core", table =>
        {
            table.HasCheckConstraint("CK_Country_ISOCode_Len", "(len(ltrim(rtrim([ISOCode])))=(2))");
            table.HasCheckConstraint("CK_Country_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.CountryId).HasName("PK_Country");

        builder.Property(x => x.CountryId).HasColumnName("CountryID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsoCode).HasColumnName("ISOCode").HasColumnType("char(2)").IsRequired();

        builder.HasIndex(x => x.IsoCode).IsUnique().HasDatabaseName("UQ_Country_ISOCode");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_Country_Name");
    }
}

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("City", "core", table =>
        {
            table.HasCheckConstraint("CK_City_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.CityId).HasName("PK_City");

        builder.Property(x => x.CityId).HasColumnName("CityID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CountryId).HasColumnName("CountryID");

        builder.HasIndex(x => new { x.CountryId, x.Name }).IsUnique().HasDatabaseName("UQ_City_CountryID_Name");
        builder.HasIndex(x => x.Name).HasDatabaseName("IX_City_Name");

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_City_Country");
    }
}

public sealed class GenderConfiguration : IEntityTypeConfiguration<Gender>
{
    public void Configure(EntityTypeBuilder<Gender> builder)
    {
        builder.ToTable("Gender", "core", table =>
        {
            table.HasCheckConstraint("CK_Gender_Code_Len", "(len(ltrim(rtrim([Code])))=(1))");
            table.HasCheckConstraint("CK_Gender_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.GenderId).HasName("PK_Gender");

        builder.Property(x => x.GenderId).HasColumnName("GenderID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Code).HasColumnType("char(1)").IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_Gender_Code");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_Gender_Name");
    }
}

public sealed class MaritalStatusConfiguration : IEntityTypeConfiguration<MaritalStatus>
{
    public void Configure(EntityTypeBuilder<MaritalStatus> builder)
    {
        builder.ToTable("MaritalStatus", "core", table =>
        {
            table.HasCheckConstraint("CK_MaritalStatus_Code_Len", "(len(ltrim(rtrim([Code])))=(1))");
            table.HasCheckConstraint("CK_MaritalStatus_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.MaritalStatusId).HasName("PK_MaritalStatus");

        builder.Property(x => x.MaritalStatusId).HasColumnName("MaritalStatusID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Code).HasColumnType("char(1)").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UQ_MaritalStatus_Code");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_MaritalStatus_Name");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_MaritalStatus_IsActive");
    }
}

public sealed class ContactTypeConfiguration : IEntityTypeConfiguration<ContactType>
{
    public void Configure(EntityTypeBuilder<ContactType> builder)
    {
        builder.ToTable("ContactType", "core", table =>
        {
            table.HasCheckConstraint("CK_ContactType_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.ContactTypeId).HasName("PK_ContactType");

        builder.Property(x => x.ContactTypeId).HasColumnName("ContactTypeID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_ContactType_Name");
    }
}

public sealed class AddressTypeConfiguration : IEntityTypeConfiguration<AddressType>
{
    public void Configure(EntityTypeBuilder<AddressType> builder)
    {
        builder.ToTable("AddressType", "core", table =>
        {
            table.HasCheckConstraint("CK_AddressType_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.AddressTypeId).HasName("PK_AddressType");

        builder.Property(x => x.AddressTypeId).HasColumnName("AddressTypeID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_AddressType_Name");
    }
}

public sealed class IdentifierTypeConfiguration : IEntityTypeConfiguration<IdentifierType>
{
    public void Configure(EntityTypeBuilder<IdentifierType> builder)
    {
        builder.ToTable("IdentifierType", "core", table =>
        {
            table.HasCheckConstraint("CK_IdentifierType_Name_NotEmpty", "(len(ltrim(rtrim([Name])))>(0))");
        });
        builder.HasKey(x => x.IdentifierTypeId).HasName("PK_IdentifierType");

        builder.Property(x => x.IdentifierTypeId).HasColumnName("IdentifierTypeID").UseIdentityColumn();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_IdentifierType_Name");
    }
}

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Person", "core", table =>
        {
            table.HasCheckConstraint("CK_Person_DeleteState", "([IsDeleted]=(0) AND [DeletedBy] IS NULL AND [DeletedDate] IS NULL OR [IsDeleted]=(1))");
            table.HasCheckConstraint("CK_Person_FirstName_NotEmpty", "(len(ltrim(rtrim([FirstName])))>(0))");
            table.HasCheckConstraint("CK_Person_LastName_NotEmpty", "(len(ltrim(rtrim([LastName])))>(0))");
            table.HasCheckConstraint("CK_Person_PreferredName_NotEmpty", "([PreferredName] IS NULL OR len(ltrim(rtrim([PreferredName])))>(0))");
            table.HasCheckConstraint("CK_Person_SecondName_NotEmpty", "([SecondName] IS NULL OR len(ltrim(rtrim([SecondName])))>(0))");
            table.HasCheckConstraint("CK_Person_ThirdName_NotEmpty", "([ThirdName] IS NULL OR len(ltrim(rtrim([ThirdName])))>(0))");
        });
        builder.HasKey(x => x.PersonId).HasName("PK_Person");

        builder.Property(x => x.PersonId).HasColumnName("PersonID").UseIdentityColumn();
        builder.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SecondName).HasMaxLength(50);
        builder.Property(x => x.ThirdName).HasMaxLength(50);
        builder.Property(x => x.LastName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PreferredName).HasMaxLength(50);
        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.GenderId).HasColumnName("GenderID");
        builder.Property(x => x.MaritalStatusId).HasColumnName("MaritalStatusID");
        builder.Property(x => x.NationalityCountryId).HasColumnName("NationalityCountryID");
        builder.Property(x => x.PhotoUrl).HasColumnName("PhotoURL").HasColumnType("nvarchar(max)");
        builder.Property(x => x.PhotoFileStorageItemId).HasColumnName("PhotoFileStorageItemID");
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedBy).HasColumnName("DeletedBy");
        builder.Property(x => x.DeletedDate).HasColumnType("datetime2(7)");
        builder.Property(x => x.CreatedBy).HasColumnName("CreatedBy");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");
        builder.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2(7)");

        builder.Property(x => x.FullName)
            .HasMaxLength(203)
            .HasComputedColumnSql(
                "ltrim(rtrim(concat([FirstName],N' ',coalesce([SecondName]+N' ',N''),coalesce([ThirdName]+N' ',N''),[LastName])))",
                stored: true);

        builder.HasIndex(x => x.CreatedBy).HasDatabaseName("IX_Person_CreatedBy");
        builder.HasIndex(x => x.DeletedBy).HasDatabaseName("IX_Person_DeletedBy");
        builder.HasIndex(x => x.GenderId).HasDatabaseName("IX_Person_GenderID");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Person_IsDeleted");
        builder.HasIndex(x => new { x.LastName, x.FirstName }).HasDatabaseName("IX_Person_LastName_FirstName");
        builder.HasIndex(x => x.MaritalStatusId).HasDatabaseName("IX_Person_MaritalStatusID");
        builder.HasIndex(x => x.ModifiedBy).HasDatabaseName("IX_Person_ModifiedBy");
        builder.HasIndex(x => x.NationalityCountryId).HasDatabaseName("IX_Person_NationalityCountryID");

        builder.HasOne(x => x.Employee)
            .WithOne(x => x.Person)
            .HasForeignKey<EmployeeEntity>(x => x.PersonId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Employee_Person");

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.NationalityCountryId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Person_Country_NationalityCountryID");

        builder.HasOne(x => x.PhotoFileStorageItem)
            .WithMany(x => x.PeopleUsingAsPhoto)
            .HasForeignKey(x => x.PhotoFileStorageItemId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Person_FileStorageItem_PhotoFileStorageItemID");

        builder.HasOne<Gender>()
            .WithMany()
            .HasForeignKey(x => x.GenderId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Person_Gender");

        builder.HasOne<MaritalStatus>()
            .WithMany()
            .HasForeignKey(x => x.MaritalStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Person_MaritalStatus");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Person_User_CreatedBy");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.DeletedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Person_User_DeletedBy");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ModifiedBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Person_User_ModifiedBy");
    }
}

public sealed class FileStorageItemConfiguration : IEntityTypeConfiguration<FileStorageItem>
{
    public void Configure(EntityTypeBuilder<FileStorageItem> builder)
    {
        builder.ToTable("FileStorageItem", "core", table =>
        {
            table.HasCheckConstraint("CK_FileStorageItem_FileHashSHA256_Length", "(len([FileHashSHA256])=(64))");
            table.HasCheckConstraint("CK_FileStorageItem_FileSizeBytes_Positive", "([FileSizeBytes]>(0))");
        });
        builder.HasKey(x => x.FileStorageItemId).HasName("PK_FileStorageItem");

        builder.Property(x => x.FileStorageItemId).HasColumnName("FileStorageItemID").UseIdentityColumn();
        builder.Property(x => x.FileCategory).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.Property(x => x.FileExtension).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FileSizeBytes);
        builder.Property(x => x.FileHashSha256).HasColumnName("FileHashSHA256").HasColumnType("char(64)").IsRequired();
        builder.Property(x => x.HashAlgorithm).HasMaxLength(20).HasDefaultValue("SHA-256").IsRequired();
        builder.Property(x => x.UploadedByUserId).HasColumnName("UploadedByUserID");
        builder.Property(x => x.UploadedAt).HasColumnType("datetime2(7)").HasDefaultValueSql("sysutcdatetime()");
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.LastIntegrityCheckAt).HasColumnType("datetime2(7)");

        builder.HasIndex(x => x.RelativePath)
            .IsUnique()
            .HasDatabaseName("UX_FileStorageItem_RelativePath");

        builder.HasIndex(x => new { x.FileCategory, x.FileHashSha256, x.FileSizeBytes })
            .IsUnique()
            .HasFilter("([IsActive]=(1))")
            .HasDatabaseName("UX_FileStorageItem_Active_Category_Hash_Size");

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedFileStorageItems)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_FileStorageItem_User_UploadedByUserID");
    }
}

public sealed class PersonContactConfiguration : IEntityTypeConfiguration<PersonContact>
{
    public void Configure(EntityTypeBuilder<PersonContact> builder)
    {
        builder.ToTable("PersonContact", "core", table =>
        {
            table.HasCheckConstraint("CK_PersonContact_ContactValue_NotEmpty", "(len(ltrim(rtrim([ContactValue])))>(0))");
        });
        builder.HasKey(x => x.ContactId).HasName("PK_PersonContact");

        builder.Property(x => x.ContactId).HasColumnName("ContactID").UseIdentityColumn();
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.ContactTypeId).HasColumnName("ContactTypeID");
        builder.Property(x => x.ContactValue).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => new { x.PersonId, x.ContactTypeId, x.ContactValue })
            .IsUnique()
            .HasDatabaseName("UQ_PersonContact_PersonID_ContactTypeID_ContactValue");

        builder.HasIndex(x => x.ContactTypeId).HasDatabaseName("IX_PersonContact_ContactTypeID");

        builder.HasIndex(x => new { x.PersonId, x.ContactTypeId })
            .IsUnique()
            .HasFilter("([IsPrimary]=(1))")
            .HasDatabaseName("IX_PersonContact_PersonID_ContactTypeID_OnePrimary");

        builder.HasIndex(x => new { x.PersonId, x.IsPrimary }).HasDatabaseName("IX_PersonContact_PersonID_IsPrimary");

        builder.HasOne(x => x.Person)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PersonContact_Person");

        builder.HasOne(x => x.ContactType)
            .WithMany()
            .HasForeignKey(x => x.ContactTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PersonContact_ContactType");
    }
}

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Address", "core", table =>
        {
            table.HasCheckConstraint("CK_Address_AddressLine1_NotEmpty", "(len(ltrim(rtrim([AddressLine1])))>(0))");
        });
        builder.HasKey(x => x.AddressId).HasName("PK_Address");

        builder.Property(x => x.AddressId).HasColumnName("AddressID").UseIdentityColumn();
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.CityId).HasColumnName("CityID");
        builder.Property(x => x.AddressLine1).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AddressLine2).HasMaxLength(50);
        builder.Property(x => x.Building).HasMaxLength(50);
        builder.Property(x => x.AddressTypeId).HasColumnName("AddressTypeID");
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);

        builder.HasIndex(x => x.AddressTypeId).HasDatabaseName("IX_Address_AddressTypeID");
        builder.HasIndex(x => x.CityId).HasDatabaseName("IX_Address_CityID");
        builder.HasIndex(x => new { x.PersonId, x.IsPrimary }).HasDatabaseName("IX_Address_PersonID_IsPrimary");

        builder.HasIndex(x => x.PersonId)
            .IsUnique()
            .HasFilter("([IsPrimary]=(1))")
            .HasDatabaseName("IX_Address_PersonID_OnePrimary");

        builder.HasOne(x => x.Person)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Address_Person");

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Address_City");

        builder.HasOne(x => x.AddressType)
            .WithMany()
            .HasForeignKey(x => x.AddressTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Address_AddressType");
    }
}

public sealed class PersonIdentifierConfiguration : IEntityTypeConfiguration<PersonIdentifier>
{
    public void Configure(EntityTypeBuilder<PersonIdentifier> builder)
    {
        builder.ToTable("PersonIdentifier", "core", table =>
        {
            table.HasCheckConstraint("CK_PersonIdentifier_DateRange", "([ExpiryDate] IS NULL OR [IssueDate] IS NULL OR [ExpiryDate]>=[IssueDate])");
            table.HasCheckConstraint("CK_PersonIdentifier_IdentifierValue_NotEmpty", "(len(ltrim(rtrim([IdentifierValue])))>(0))");
        });
        builder.HasKey(x => x.PersonIdentifierId).HasName("PK_PersonIdentifier");

        builder.Property(x => x.PersonIdentifierId).HasColumnName("PersonIdentifierID").UseIdentityColumn();
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.IdentifierTypeId).HasColumnName("IdentifierTypeID");
        builder.Property(x => x.IdentifierValue).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);
        builder.Property(x => x.IssueDate).HasColumnType("date");
        builder.Property(x => x.ExpiryDate).HasColumnType("date");
        builder.Property(x => x.CountryId).HasColumnName("CountryID");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2(7)").HasDefaultValueSql("sysdatetime()");

        builder.HasIndex(x => x.CountryId).HasDatabaseName("IX_PersonIdentifier_CountryID");
        builder.HasIndex(x => x.IdentifierTypeId).HasDatabaseName("IX_PersonIdentifier_IdentifierTypeID");
        builder.HasIndex(x => new { x.PersonId, x.IdentifierTypeId }, "IX_PersonIdentifier_PersonID_IdentifierTypeID")
            .HasDatabaseName("IX_PersonIdentifier_PersonID_IdentifierTypeID");

        builder.HasIndex(x => new { x.PersonId, x.IdentifierTypeId }, "IX_PersonIdentifier_PersonID_IdentifierTypeID_OnePrimary")
            .IsUnique()
            .HasFilter("([IsPrimary]=(1))")
            .HasDatabaseName("IX_PersonIdentifier_PersonID_IdentifierTypeID_OnePrimary");

        builder.HasOne(x => x.Person)
            .WithMany(x => x.Identifiers)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PersonIdentifier_Person");

        builder.HasOne(x => x.IdentifierType)
            .WithMany()
            .HasForeignKey(x => x.IdentifierTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PersonIdentifier_IdentifierType");

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_PersonIdentifier_Country");
    }
}
