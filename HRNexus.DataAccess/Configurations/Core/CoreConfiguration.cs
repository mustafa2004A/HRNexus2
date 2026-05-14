using HRNexus.DataAccess.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Configurations.Core;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Country", "core");
        builder.HasKey(x => x.CountryId);

        builder.Property(x => x.CountryId).HasColumnName("CountryID");
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsoCode).HasColumnName("ISOCode").HasMaxLength(2).IsFixedLength().IsRequired();
    }
}

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("City", "core");
        builder.HasKey(x => x.CityId);

        builder.Property(x => x.CityId).HasColumnName("CityID");
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CountryId).HasColumnName("CountryID");

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class GenderConfiguration : IEntityTypeConfiguration<Gender>
{
    public void Configure(EntityTypeBuilder<Gender> builder)
    {
        builder.ToTable("Gender", "core");
        builder.HasKey(x => x.GenderId);

        builder.Property(x => x.GenderId).HasColumnName("GenderID");
        builder.Property(x => x.Name).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(1).IsFixedLength().IsRequired();
    }
}

public sealed class MaritalStatusConfiguration : IEntityTypeConfiguration<MaritalStatus>
{
    public void Configure(EntityTypeBuilder<MaritalStatus> builder)
    {
        builder.ToTable("MaritalStatus", "core");
        builder.HasKey(x => x.MaritalStatusId);

        builder.Property(x => x.MaritalStatusId).HasColumnName("MaritalStatusID");
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(1).IsFixedLength().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}

public sealed class ContactTypeConfiguration : IEntityTypeConfiguration<ContactType>
{
    public void Configure(EntityTypeBuilder<ContactType> builder)
    {
        builder.ToTable("ContactType", "core");
        builder.HasKey(x => x.ContactTypeId);

        builder.Property(x => x.ContactTypeId).HasColumnName("ContactTypeID");
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
    }
}

public sealed class AddressTypeConfiguration : IEntityTypeConfiguration<AddressType>
{
    public void Configure(EntityTypeBuilder<AddressType> builder)
    {
        builder.ToTable("AddressType", "core");
        builder.HasKey(x => x.AddressTypeId);

        builder.Property(x => x.AddressTypeId).HasColumnName("AddressTypeID");
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
    }
}

public sealed class IdentifierTypeConfiguration : IEntityTypeConfiguration<IdentifierType>
{
    public void Configure(EntityTypeBuilder<IdentifierType> builder)
    {
        builder.ToTable("IdentifierType", "core");
        builder.HasKey(x => x.IdentifierTypeId);

        builder.Property(x => x.IdentifierTypeId).HasColumnName("IdentifierTypeID");
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
    }
}

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Person", "core");
        builder.HasKey(x => x.PersonId);

        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SecondName).HasMaxLength(50);
        builder.Property(x => x.ThirdName).HasMaxLength(50);
        builder.Property(x => x.LastName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PreferredName).HasMaxLength(50);
        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.GenderId).HasColumnName("GenderID");
        builder.Property(x => x.MaritalStatusId).HasColumnName("MaritalStatusID");
        builder.Property(x => x.NationalityCountryId).HasColumnName("NationalityCountryID");
        builder.Property(x => x.PhotoUrl).HasColumnName("PhotoURL");
        builder.Property(x => x.PhotoFileStorageItemId).HasColumnName("PhotoFileStorageItemID");
        builder.Property(x => x.DeletedBy).HasColumnName("DeletedBy");
        builder.Property(x => x.DeletedDate).HasColumnType("datetime2");
        builder.Property(x => x.CreatedBy).HasColumnName("CreatedBy");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");
        builder.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy");
        builder.Property(x => x.ModifiedDate).HasColumnType("datetime2");

        builder.Property(x => x.FullName)
            .HasMaxLength(203)
            .HasComputedColumnSql(
                "LTRIM(RTRIM(CONCAT(FirstName, N' ', COALESCE(SecondName + N' ', N''), COALESCE(ThirdName + N' ', N''), LastName)))",
                stored: true);

        builder.HasOne(x => x.Employee)
            .WithOne(x => x.Person)
            .HasForeignKey<EmployeeEntity>(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PhotoFileStorageItem)
            .WithMany(x => x.PeopleUsingAsPhoto)
            .HasForeignKey(x => x.PhotoFileStorageItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class FileStorageItemConfiguration : IEntityTypeConfiguration<FileStorageItem>
{
    public void Configure(EntityTypeBuilder<FileStorageItem> builder)
    {
        builder.ToTable("FileStorageItem", "core");
        builder.HasKey(x => x.FileStorageItemId);

        builder.Property(x => x.FileStorageItemId).HasColumnName("FileStorageItemID");
        builder.Property(x => x.FileCategory).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.Property(x => x.FileExtension).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FileHashSha256).HasColumnName("FileHashSHA256").HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.HashAlgorithm).HasMaxLength(20).IsRequired();
        builder.Property(x => x.UploadedByUserId).HasColumnName("UploadedByUserID");
        builder.Property(x => x.UploadedAt).HasColumnType("datetime2");
        builder.Property(x => x.LastIntegrityCheckAt).HasColumnType("datetime2");

        builder.HasIndex(x => x.RelativePath)
            .IsUnique();

        builder.HasIndex(x => new { x.FileCategory, x.FileHashSha256, x.FileSizeBytes })
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedFileStorageItems)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PersonContactConfiguration : IEntityTypeConfiguration<PersonContact>
{
    public void Configure(EntityTypeBuilder<PersonContact> builder)
    {
        builder.ToTable("PersonContact", "core");
        builder.HasKey(x => x.ContactId);

        builder.Property(x => x.ContactId).HasColumnName("ContactID");
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.ContactTypeId).HasColumnName("ContactTypeID");
        builder.Property(x => x.ContactValue).HasMaxLength(255).IsRequired();
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");

        builder.HasOne(x => x.Person)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContactType)
            .WithMany()
            .HasForeignKey(x => x.ContactTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Address", "core");
        builder.HasKey(x => x.AddressId);

        builder.Property(x => x.AddressId).HasColumnName("AddressID");
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.CityId).HasColumnName("CityID");
        builder.Property(x => x.AddressLine1).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AddressLine2).HasMaxLength(50);
        builder.Property(x => x.Building).HasMaxLength(50);
        builder.Property(x => x.AddressTypeId).HasColumnName("AddressTypeID");

        builder.HasOne(x => x.Person)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AddressType)
            .WithMany()
            .HasForeignKey(x => x.AddressTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PersonIdentifierConfiguration : IEntityTypeConfiguration<PersonIdentifier>
{
    public void Configure(EntityTypeBuilder<PersonIdentifier> builder)
    {
        builder.ToTable("PersonIdentifier", "core");
        builder.HasKey(x => x.PersonIdentifierId);

        builder.Property(x => x.PersonIdentifierId).HasColumnName("PersonIdentifierID");
        builder.Property(x => x.PersonId).HasColumnName("PersonID");
        builder.Property(x => x.IdentifierTypeId).HasColumnName("IdentifierTypeID");
        builder.Property(x => x.IdentifierValue).IsRequired();
        builder.Property(x => x.IssueDate).HasColumnType("date");
        builder.Property(x => x.ExpiryDate).HasColumnType("date");
        builder.Property(x => x.CountryId).HasColumnName("CountryID");
        builder.Property(x => x.CreatedDate).HasColumnType("datetime2");

        builder.HasOne(x => x.Person)
            .WithMany(x => x.Identifiers)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IdentifierType)
            .WithMany()
            .HasForeignKey(x => x.IdentifierTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
