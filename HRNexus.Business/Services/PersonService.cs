using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Core;
using HRNexus.Business.Models.Files;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;

    public PersonService(
        IPersonRepository personRepository,
        IReferenceDataRepository referenceDataRepository,
        IFileStorageService fileStorageService,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext)
    {
        _personRepository = personRepository;
        _referenceDataRepository = referenceDataRepository;
        _fileStorageService = fileStorageService;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PersonDto>> ListAsync(
        string? search,
        bool includeDeleted,
        CancellationToken cancellationToken = default)
    {
        var people = await _personRepository.ListAsync(search, includeDeleted, cancellationToken);
        return people.Select(OperationalServiceHelpers.ToPersonDto).ToList();
    }

    public async Task<PersonDto> GetByIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken: cancellationToken)
            ?? throw PersonNotFound(personId);

        return OperationalServiceHelpers.ToPersonDto(person);
    }

    public async Task<PersonDto> CreateAsync(CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidatePersonReferencesAsync(request, cancellationToken);

        var person = CreatePersonEntity(request);
        var now = DateTime.UtcNow;
        person.CreatedDate = now;
        person.CreatedBy = _currentUserContext.UserId;

        await _personRepository.AddAsync(person, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create person", cancellationToken);

        var created = await _personRepository.GetByIdAsync(person.PersonId, includeDeleted: true, cancellationToken)
            ?? throw PersonNotFound(person.PersonId);

        return OperationalServiceHelpers.ToPersonDto(created);
    }

    public async Task<PersonDto> UpdateAsync(int personId, UpdatePersonRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidatePersonReferencesAsync(request, cancellationToken);

        var person = await _personRepository.GetByIdForUpdateAsync(personId, cancellationToken)
            ?? throw PersonNotFound(personId);

        if (person.IsDeleted)
        {
            throw PersonNotFound(personId);
        }

        ApplyPersonRequest(person, request);
        person.ModifiedBy = _currentUserContext.UserId;
        person.ModifiedDate = DateTime.UtcNow;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update person", cancellationToken);

        var updated = await _personRepository.GetByIdAsync(personId, includeDeleted: true, cancellationToken)
            ?? throw PersonNotFound(personId);

        return OperationalServiceHelpers.ToPersonDto(updated);
    }

    public async Task<PersonPhotoDto> UploadPhotoAsync(
        int personId,
        FileUploadContent file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        var person = await _personRepository.GetByIdForUpdateAsync(personId, cancellationToken)
            ?? throw PersonNotFound(personId);

        if (person.IsDeleted)
        {
            throw PersonNotFound(personId);
        }

        var storedFile = await _fileStorageService.SaveAsync(
            FileStorageCategories.PersonPhoto,
            file,
            _currentUserContext.UserId,
            cancellationToken);

        person.PhotoFileStorageItemId = storedFile.FileStorageItemId;
        person.PhotoUrl = storedFile.RelativePath;
        person.ModifiedBy = _currentUserContext.UserId;
        person.ModifiedDate = DateTime.UtcNow;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "upload person photo", cancellationToken);

        return new PersonPhotoDto(
            person.PersonId,
            storedFile.FileStorageItemId,
            storedFile.RelativePath,
            storedFile);
    }

    public async Task<PersonDto> DeleteAsync(int personId, CancellationToken cancellationToken = default)
    {
        var person = await _personRepository.GetByIdForUpdateAsync(personId, cancellationToken)
            ?? throw PersonNotFound(personId);

        if (person.IsDeleted)
        {
            throw PersonNotFound(personId);
        }

        person.IsDeleted = true;
        person.DeletedBy = _currentUserContext.UserId;
        person.DeletedDate = DateTime.UtcNow;
        person.ModifiedBy = _currentUserContext.UserId;
        person.ModifiedDate = person.DeletedDate;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "soft-delete person", cancellationToken);
        return OperationalServiceHelpers.ToPersonDto(person);
    }

    internal static Person CreatePersonEntity(CreatePersonRequest request)
    {
        var person = new Person();
        ApplyPersonRequest(person, request);
        return person;
    }

    internal static void ApplyPersonRequest(Person person, CreatePersonRequest request)
    {
        person.FirstName = OperationalServiceHelpers.RequiredText(request.FirstName, "First name");
        person.SecondName = OperationalServiceHelpers.OptionalText(request.SecondName);
        person.ThirdName = OperationalServiceHelpers.OptionalText(request.ThirdName);
        person.LastName = OperationalServiceHelpers.RequiredText(request.LastName, "Last name");
        person.PreferredName = OperationalServiceHelpers.OptionalText(request.PreferredName);
        person.DateOfBirth = request.DateOfBirth;
        person.GenderId = request.GenderId;
        person.MaritalStatusId = request.MaritalStatusId;
        person.NationalityCountryId = request.NationalityCountryId;
    }

    internal async Task ValidatePersonReferencesAsync(CreatePersonRequest request, CancellationToken cancellationToken)
    {
        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BusinessRuleException("Date of birth cannot be in the future.");
        }

        if (request.GenderId.HasValue
            && !await _referenceDataRepository.GenderExistsAsync(request.GenderId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Gender {request.GenderId.Value} was not found.");
        }

        if (request.MaritalStatusId.HasValue
            && !await _referenceDataRepository.MaritalStatusExistsAsync(request.MaritalStatusId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Marital status {request.MaritalStatusId.Value} was not found.");
        }

        if (request.NationalityCountryId.HasValue
            && !await _referenceDataRepository.CountryExistsAsync(request.NationalityCountryId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Nationality country {request.NationalityCountryId.Value} was not found.");
        }
    }

    private static EntityNotFoundException PersonNotFound(int personId)
    {
        return new EntityNotFoundException($"Person {personId} was not found.");
    }
}
