using HRNexus.DataAccess.Entities.Employee;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Entities.Core;

public sealed class Person
{
    public int PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string? ThirdName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? PreferredName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? GenderId { get; set; }
    public int? MaritalStatusId { get; set; }
    public int? NationalityCountryId { get; set; }
    public string? PhotoUrl { get; set; }
    public int? PhotoFileStorageItemId { get; set; }
    public bool IsDeleted { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public EmployeeEntity? Employee { get; set; }
    public FileStorageItem? PhotoFileStorageItem { get; set; }
    public ICollection<PersonContact> Contacts { get; set; } = new List<PersonContact>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<PersonIdentifier> Identifiers { get; set; } = new List<PersonIdentifier>();
}

public sealed class PersonContact
{
    public int ContactId { get; set; }
    public int PersonId { get; set; }
    public int ContactTypeId { get; set; }
    public string ContactValue { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateTime CreatedDate { get; set; }

    public Person Person { get; set; } = null!;
    public ContactType ContactType { get; set; } = null!;
}

public sealed class Address
{
    public int AddressId { get; set; }
    public int PersonId { get; set; }
    public int CityId { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? Building { get; set; }
    public int AddressTypeId { get; set; }
    public bool IsPrimary { get; set; }

    public Person Person { get; set; } = null!;
    public City City { get; set; } = null!;
    public AddressType AddressType { get; set; } = null!;
}

public sealed class PersonIdentifier
{
    public int PersonIdentifierId { get; set; }
    public int PersonId { get; set; }
    public int IdentifierTypeId { get; set; }
    public string IdentifierValue { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public int? CountryId { get; set; }
    public DateTime CreatedDate { get; set; }

    public Person Person { get; set; } = null!;
    public IdentifierType IdentifierType { get; set; } = null!;
    public Country? Country { get; set; }
}
