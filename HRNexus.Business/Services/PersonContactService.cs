using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Core;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class PersonContactService : IPersonContactService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonContactRepository _contactRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IHRNexusDbContext _dbContext;

    public PersonContactService(
        IPersonRepository personRepository,
        IPersonContactRepository contactRepository,
        IReferenceDataRepository referenceDataRepository,
        IHRNexusDbContext dbContext)
    {
        _personRepository = personRepository;
        _contactRepository = contactRepository;
        _referenceDataRepository = referenceDataRepository;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PersonContactDto>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var contacts = await _contactRepository.GetByPersonAsync(personId, cancellationToken);
        return contacts.Select(OperationalServiceHelpers.ToContactDto).ToList();
    }

    public async Task<PersonContactDto> GetByIdAsync(int personId, int contactId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var contact = await _contactRepository.GetByIdAsync(personId, contactId, cancellationToken)
            ?? throw ContactNotFound(contactId);

        return OperationalServiceHelpers.ToContactDto(contact);
    }

    public async Task<PersonContactDto> CreateAsync(int personId, CreatePersonContactRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsurePersonExistsAsync(personId, cancellationToken);
        await EnsureContactTypeExistsAsync(request.ContactTypeId, cancellationToken);

        if (request.IsPrimary)
        {
            await _contactRepository.DemotePrimaryContactsAsync(personId, request.ContactTypeId, cancellationToken: cancellationToken);
        }

        var contact = new PersonContact
        {
            PersonId = personId,
            ContactTypeId = request.ContactTypeId,
            ContactValue = OperationalServiceHelpers.RequiredText(request.ContactValue, "Contact value"),
            IsPrimary = request.IsPrimary,
            CreatedDate = DateTime.UtcNow
        };

        await _contactRepository.AddAsync(contact, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create person contact", cancellationToken);

        return await GetByIdAsync(personId, contact.ContactId, cancellationToken);
    }

    public async Task<PersonContactDto> UpdateAsync(int personId, int contactId, UpdatePersonContactRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsurePersonExistsAsync(personId, cancellationToken);
        await EnsureContactTypeExistsAsync(request.ContactTypeId, cancellationToken);

        var contact = await _contactRepository.GetByIdForUpdateAsync(personId, contactId, cancellationToken)
            ?? throw ContactNotFound(contactId);

        if (request.IsPrimary)
        {
            await _contactRepository.DemotePrimaryContactsAsync(personId, request.ContactTypeId, contactId, cancellationToken);
        }

        contact.ContactTypeId = request.ContactTypeId;
        contact.ContactValue = OperationalServiceHelpers.RequiredText(request.ContactValue, "Contact value");
        contact.IsPrimary = request.IsPrimary;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update person contact", cancellationToken);
        return await GetByIdAsync(personId, contactId, cancellationToken);
    }

    public async Task<PersonContactDto> DeleteAsync(int personId, int contactId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var existing = await GetByIdAsync(personId, contactId, cancellationToken);
        var contact = await _contactRepository.GetByIdForUpdateAsync(personId, contactId, cancellationToken)
            ?? throw ContactNotFound(contactId);

        _contactRepository.Remove(contact);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "delete person contact", cancellationToken);
        return existing;
    }

    private async Task EnsurePersonExistsAsync(int personId, CancellationToken cancellationToken)
    {
        if (!await _personRepository.ExistsAsync(personId, cancellationToken: cancellationToken))
        {
            throw new EntityNotFoundException($"Person {personId} was not found.");
        }
    }

    private async Task EnsureContactTypeExistsAsync(int contactTypeId, CancellationToken cancellationToken)
    {
        if (!await _referenceDataRepository.ContactTypeExistsAsync(contactTypeId, cancellationToken))
        {
            throw new BusinessRuleException($"Contact type {contactTypeId} was not found.");
        }
    }

    private static EntityNotFoundException ContactNotFound(int contactId)
    {
        return new EntityNotFoundException($"Person contact {contactId} was not found.");
    }
}
