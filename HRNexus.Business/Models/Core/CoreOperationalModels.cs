using System.ComponentModel.DataAnnotations;
using HRNexus.Business.Models.Files;

namespace HRNexus.Business.Models.Core;

public sealed record PersonDto(
    int PersonId,
    string FirstName,
    string? SecondName,
    string? ThirdName,
    string LastName,
    string FullName,
    string? PreferredName,
    DateOnly? DateOfBirth,
    int? GenderId,
    int? MaritalStatusId,
    int? NationalityCountryId,
    string? PhotoUrl,
    bool IsDeleted);

public sealed record PersonPhotoDto(
    int PersonId,
    int FileStorageItemId,
    string PhotoUrl,
    FileStorageItemDto File);

public class CreatePersonRequest
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SecondName { get; set; }

    [StringLength(50)]
    public string? ThirdName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PreferredName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Range(1, int.MaxValue)]
    public int? GenderId { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaritalStatusId { get; set; }

    [Range(1, int.MaxValue)]
    public int? NationalityCountryId { get; set; }

}

public sealed class UpdatePersonRequest : CreatePersonRequest;

public sealed record PersonContactDto(
    int ContactId,
    int PersonId,
    int ContactTypeId,
    string ContactTypeName,
    string ContactValue,
    bool IsPrimary,
    DateTime CreatedDate);

public class CreatePersonContactRequest
{
    [Range(1, int.MaxValue)]
    public int ContactTypeId { get; set; }

    [Required]
    [StringLength(255)]
    public string ContactValue { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}

public sealed class UpdatePersonContactRequest : CreatePersonContactRequest;

public sealed record AddressDto(
    int AddressId,
    int PersonId,
    int CityId,
    string CityName,
    int AddressTypeId,
    string AddressTypeName,
    string AddressLine1,
    string? AddressLine2,
    string? Building,
    bool IsPrimary);

public class CreateAddressRequest
{
    [Range(1, int.MaxValue)]
    public int CityId { get; set; }

    [Range(1, int.MaxValue)]
    public int AddressTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(50)]
    public string? AddressLine2 { get; set; }

    [StringLength(50)]
    public string? Building { get; set; }

    public bool IsPrimary { get; set; }
}

public sealed class UpdateAddressRequest : CreateAddressRequest;

public sealed record PersonIdentifierDto(
    int PersonIdentifierId,
    int PersonId,
    int IdentifierTypeId,
    string IdentifierTypeName,
    string MaskedIdentifierValue,
    bool IsPrimary,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    int? CountryId,
    string? CountryName,
    DateTime CreatedDate);

public class CreatePersonIdentifierRequest
{
    [Range(1, int.MaxValue)]
    public int IdentifierTypeId { get; set; }

    [Required]
    [StringLength(512)]
    public string IdentifierValue { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    [Range(1, int.MaxValue)]
    public int? CountryId { get; set; }
}

public sealed class UpdatePersonIdentifierRequest : CreatePersonIdentifierRequest;
