using HRNexus.Business.Exceptions;
using HRNexus.Business.Models.Core;
using HRNexus.Business.Models.Employee;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Core;
using HRNexus.DataAccess.Repositories.Employee;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.Business.Services;

internal static class OperationalServiceHelpers
{
    public static string RequiredText(string value, string fieldName)
    {
        return BusinessValidation.NormalizeRequiredText(value, fieldName);
    }

    public static string? OptionalText(string? value)
    {
        return BusinessValidation.NormalizeOptionalText(value);
    }

    public static async Task SaveChangesAsync(
        IHRNexusDbContext dbContext,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new BusinessRuleException(
                $"Unable to {operationDescription}. Check duplicate values, related references, or records already in use.",
                exception);
        }
    }

    public static PersonDto ToPersonDto(PersonQueryResult person)
    {
        return new PersonDto(
            person.PersonId,
            person.FirstName,
            person.SecondName,
            person.ThirdName,
            person.LastName,
            person.FullName,
            person.PreferredName,
            person.DateOfBirth,
            person.GenderId,
            person.MaritalStatusId,
            person.NationalityCountryId,
            ToSafeNullableRelativeFilePath(person.PhotoUrl),
            person.IsDeleted);
    }

    public static PersonDto ToPersonDto(Person person)
    {
        return new PersonDto(
            person.PersonId,
            person.FirstName,
            person.SecondName,
            person.ThirdName,
            person.LastName,
            person.FullName,
            person.PreferredName,
            person.DateOfBirth,
            person.GenderId,
            person.MaritalStatusId,
            person.NationalityCountryId,
            ToSafeNullableRelativeFilePath(person.PhotoUrl),
            person.IsDeleted);
    }

    public static PersonContactDto ToContactDto(PersonContactQueryResult contact)
    {
        return new PersonContactDto(
            contact.ContactId,
            contact.PersonId,
            contact.ContactTypeId,
            contact.ContactTypeName,
            contact.ContactValue,
            contact.IsPrimary,
            contact.CreatedDate);
    }

    public static AddressDto ToAddressDto(AddressQueryResult address)
    {
        return new AddressDto(
            address.AddressId,
            address.PersonId,
            address.CityId,
            address.CityName,
            address.AddressTypeId,
            address.AddressTypeName,
            address.AddressLine1,
            address.AddressLine2,
            address.Building,
            address.IsPrimary);
    }

    public static PersonIdentifierDto ToIdentifierDto(PersonIdentifierQueryResult identifier)
    {
        return new PersonIdentifierDto(
            identifier.PersonIdentifierId,
            identifier.PersonId,
            identifier.IdentifierTypeId,
            identifier.IdentifierTypeName,
            MaskSensitiveValue(identifier.IdentifierValue),
            identifier.IsPrimary,
            identifier.IssueDate,
            identifier.ExpiryDate,
            identifier.CountryId,
            identifier.CountryName,
            identifier.CreatedDate);
    }

    public static EmployeeSummaryDto ToEmployeeSummaryDto(EmployeeSummaryQueryResult employee)
    {
        return new EmployeeSummaryDto(
            employee.EmployeeId,
            employee.PersonId,
            employee.EmployeeCode,
            employee.FullName,
            employee.HireDate,
            employee.CurrentEmploymentStatusId,
            employee.EmploymentStatusName,
            employee.IsDeleted);
    }

    public static EmployeeJobHistoryDto ToEmployeeJobHistoryDto(EmployeeJobHistoryQueryResult job)
    {
        return new EmployeeJobHistoryDto(
            job.JobHistoryId,
            job.EmployeeId,
            job.DepartmentId,
            job.DepartmentName,
            job.PositionId,
            job.PositionName,
            job.EmploymentTypeId,
            job.EmploymentTypeName,
            job.JobGradeId,
            job.JobGradeName,
            job.EmploymentStatusId,
            job.EmploymentStatusName,
            job.ManagerId,
            job.ManagerName,
            job.IsCurrent,
            job.StartDate,
            job.EndDate);
    }

    public static EmployeeDocumentDto ToEmployeeDocumentDto(EmployeeDocumentQueryResult document)
    {
        return new EmployeeDocumentDto(
            document.DocumentId,
            document.EmployeeId,
            document.DocumentTypeId,
            document.DocumentTypeName,
            document.DocumentName,
            document.ReferenceNumber,
            ToSafeRelativeFilePath(document.FilePath),
            document.FileExtension,
            document.FileStorageItemId,
            document.IssueDate,
            document.ExpiryDate,
            document.IsVerified,
            document.VerifiedBy,
            document.VerifiedByUsername,
            document.VerifiedDate,
            document.IsActive,
            document.UploadedBy,
            document.UploadedByUsername,
            document.UploadedDate,
            document.Remarks);
    }

    public static string MaskSensitiveValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length <= 4)
        {
            return new string('*', trimmed.Length);
        }

        return $"{new string('*', Math.Min(8, trimmed.Length - 4))}{trimmed[^4..]}";
    }

    public static string ToSafeRelativeFilePath(string? path)
    {
        return ToSafeNullableRelativeFilePath(path) ?? string.Empty;
    }

    public static string? ToSafeNullableRelativeFilePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var trimmed = path.Trim();
        var normalized = trimmed.Replace('\\', '/');

        if (Path.IsPathRooted(trimmed)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || normalized.Contains(':', StringComparison.Ordinal)
            || Uri.TryCreate(trimmed, UriKind.Absolute, out _))
        {
            return null;
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(segment => segment == "." || segment == ".."))
        {
            return null;
        }

        return normalized;
    }
}
