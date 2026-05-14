using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Core;

public sealed class PersonContactRepository : IPersonContactRepository
{
    private readonly HRNexusDbContext _dbContext;

    public PersonContactRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PersonContactQueryResult>> GetByPersonAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        return await CreateContactQuery()
            .Where(contact => contact.PersonId == personId)
            .OrderByDescending(contact => contact.IsPrimary)
            .ThenBy(contact => contact.ContactType.Name)
            .ThenBy(contact => contact.ContactValue)
            .Select(contact => new PersonContactQueryResult(
                contact.ContactId,
                contact.PersonId,
                contact.ContactTypeId,
                contact.ContactType.Name,
                contact.ContactValue,
                contact.IsPrimary,
                contact.CreatedDate))
            .ToListAsync(cancellationToken);
    }

    public Task<PersonContactQueryResult?> GetByIdAsync(
        int personId,
        int contactId,
        CancellationToken cancellationToken = default)
    {
        return CreateContactQuery()
            .Where(contact => contact.PersonId == personId && contact.ContactId == contactId)
            .Select(contact => new PersonContactQueryResult(
                contact.ContactId,
                contact.PersonId,
                contact.ContactTypeId,
                contact.ContactType.Name,
                contact.ContactValue,
                contact.IsPrimary,
                contact.CreatedDate))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<PersonContact?> GetByIdForUpdateAsync(
        int personId,
        int contactId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.PersonContacts
            .FirstOrDefaultAsync(contact => contact.PersonId == personId && contact.ContactId == contactId, cancellationToken);
    }

    public Task AddAsync(PersonContact contact, CancellationToken cancellationToken = default)
    {
        return _dbContext.PersonContacts.AddAsync(contact, cancellationToken).AsTask();
    }

    public void Remove(PersonContact contact)
    {
        _dbContext.PersonContacts.Remove(contact);
    }

    public async Task DemotePrimaryContactsAsync(
        int personId,
        int contactTypeId,
        int? exceptContactId = null,
        CancellationToken cancellationToken = default)
    {
        var contacts = await _dbContext.PersonContacts
            .Where(contact =>
                contact.PersonId == personId
                && contact.ContactTypeId == contactTypeId
                && contact.IsPrimary
                && (!exceptContactId.HasValue || contact.ContactId != exceptContactId.Value))
            .ToListAsync(cancellationToken);

        foreach (var contact in contacts)
        {
            contact.IsPrimary = false;
        }
    }

    private IQueryable<PersonContact> CreateContactQuery()
    {
        return _dbContext.PersonContacts.AsNoTracking();
    }

}
