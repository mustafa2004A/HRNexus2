namespace HRNexus.DataAccess.Entities.Core;

public sealed class Country
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;

    public ICollection<City> Cities { get; set; } = new List<City>();
}

public sealed class City
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;
}

public sealed class Gender
{
    public int GenderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public sealed class MaritalStatus
{
    public int MaritalStatusId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

public sealed class ContactType
{
    public int ContactTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class AddressType
{
    public int AddressTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class IdentifierType
{
    public int IdentifierTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
}
