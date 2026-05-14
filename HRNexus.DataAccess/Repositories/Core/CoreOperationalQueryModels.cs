namespace HRNexus.DataAccess.Repositories.Core;

public sealed record PersonQueryResult(
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

public sealed record PersonContactQueryResult(
    int ContactId,
    int PersonId,
    int ContactTypeId,
    string ContactTypeName,
    string ContactValue,
    bool IsPrimary,
    DateTime CreatedDate);

public sealed record AddressQueryResult(
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

public sealed record PersonIdentifierQueryResult(
    int PersonIdentifierId,
    int PersonId,
    int IdentifierTypeId,
    string IdentifierTypeName,
    string IdentifierValue,
    bool IsPrimary,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    int? CountryId,
    string? CountryName,
    DateTime CreatedDate);
