using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HRNexus.DataAccess.Repositories.Core;

public sealed class PersonRepository : IPersonRepository
{
    private readonly HRNexusDbContext _dbContext;

    public PersonRepository(HRNexusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PersonQueryResult>> ListAsync(
        string? search,
        bool includeDeleted,
        CancellationToken cancellationToken = default)
    {
        var query = CreatePersonQuery(includeDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(person =>
                person.FirstName.Contains(trimmedSearch)
                || person.LastName.Contains(trimmedSearch)
                || person.FullName.Contains(trimmedSearch)
                || (person.PreferredName != null && person.PreferredName.Contains(trimmedSearch)));
        }

        return await query
            .OrderBy(person => person.LastName)
            .ThenBy(person => person.FirstName)
            .ThenBy(person => person.PersonId)
            .Select(person => new PersonQueryResult(
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
                person.PhotoUrl,
                person.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    public Task<PersonQueryResult?> GetByIdAsync(
        int personId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return CreatePersonQuery(includeDeleted)
            .Where(person => person.PersonId == personId)
            .Select(person => new PersonQueryResult(
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
                person.PhotoUrl,
                person.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Person?> GetByIdForUpdateAsync(int personId, CancellationToken cancellationToken = default)
    {
        return _dbContext.People
            .FirstOrDefaultAsync(person => person.PersonId == personId, cancellationToken);
    }

    public Task<bool> ExistsAsync(int personId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return CreatePersonQuery(includeDeleted)
            .AnyAsync(person => person.PersonId == personId, cancellationToken);
    }

    public Task AddAsync(Person person, CancellationToken cancellationToken = default)
    {
        return _dbContext.People.AddAsync(person, cancellationToken).AsTask();
    }

    private IQueryable<Person> CreatePersonQuery(bool includeDeleted)
    {
        var query = _dbContext.People.AsNoTracking();
        return includeDeleted ? query : query.Where(person => !person.IsDeleted);
    }

}
