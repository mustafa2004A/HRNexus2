using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Core;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class PersonIdentifierService : IPersonIdentifierService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonIdentifierRepository _identifierRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IHRNexusDbContext _dbContext;

    public PersonIdentifierService(
        IPersonRepository personRepository,
        IPersonIdentifierRepository identifierRepository,
        IReferenceDataRepository referenceDataRepository,
        IHRNexusDbContext dbContext)
    {
        _personRepository = personRepository;
        _identifierRepository = identifierRepository;
        _referenceDataRepository = referenceDataRepository;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PersonIdentifierDto>> GetByPersonAsync(int personId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var identifiers = await _identifierRepository.GetByPersonAsync(personId, cancellationToken);
        return identifiers.Select(OperationalServiceHelpers.ToIdentifierDto).ToList();
    }

    public async Task<PersonIdentifierDto> GetByIdAsync(int personId, int identifierId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var identifier = await _identifierRepository.GetByIdAsync(personId, identifierId, cancellationToken)
            ?? throw IdentifierNotFound(identifierId);

        return OperationalServiceHelpers.ToIdentifierDto(identifier);
    }

    public async Task<PersonIdentifierDto> CreateAsync(int personId, CreatePersonIdentifierRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateAsync(personId, request.IdentifierTypeId, request.CountryId, request.IssueDate, request.ExpiryDate, cancellationToken);

        if (request.IsPrimary)
        {
            await _identifierRepository.DemotePrimaryIdentifiersAsync(personId, request.IdentifierTypeId, cancellationToken: cancellationToken);
        }

        var identifier = new PersonIdentifier
        {
            PersonId = personId,
            IdentifierTypeId = request.IdentifierTypeId,
            IdentifierValue = OperationalServiceHelpers.RequiredText(request.IdentifierValue, "Identifier value"),
            IsPrimary = request.IsPrimary,
            IssueDate = request.IssueDate,
            ExpiryDate = request.ExpiryDate,
            CountryId = request.CountryId,
            CreatedDate = DateTime.UtcNow
        };

        await _identifierRepository.AddAsync(identifier, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create person identifier", cancellationToken);
        return await GetByIdAsync(personId, identifier.PersonIdentifierId, cancellationToken);
    }

    public async Task<PersonIdentifierDto> UpdateAsync(int personId, int identifierId, UpdatePersonIdentifierRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await ValidateAsync(personId, request.IdentifierTypeId, request.CountryId, request.IssueDate, request.ExpiryDate, cancellationToken);

        var identifier = await _identifierRepository.GetByIdForUpdateAsync(personId, identifierId, cancellationToken)
            ?? throw IdentifierNotFound(identifierId);

        if (request.IsPrimary)
        {
            await _identifierRepository.DemotePrimaryIdentifiersAsync(personId, request.IdentifierTypeId, identifierId, cancellationToken);
        }

        identifier.IdentifierTypeId = request.IdentifierTypeId;
        identifier.IdentifierValue = OperationalServiceHelpers.RequiredText(request.IdentifierValue, "Identifier value");
        identifier.IsPrimary = request.IsPrimary;
        identifier.IssueDate = request.IssueDate;
        identifier.ExpiryDate = request.ExpiryDate;
        identifier.CountryId = request.CountryId;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update person identifier", cancellationToken);
        return await GetByIdAsync(personId, identifierId, cancellationToken);
    }

    public async Task<PersonIdentifierDto> DeleteAsync(int personId, int identifierId, CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);
        var existing = await GetByIdAsync(personId, identifierId, cancellationToken);
        var identifier = await _identifierRepository.GetByIdForUpdateAsync(personId, identifierId, cancellationToken)
            ?? throw IdentifierNotFound(identifierId);

        _identifierRepository.Remove(identifier);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "delete person identifier", cancellationToken);
        return existing;
    }

    private async Task ValidateAsync(
        int personId,
        int identifierTypeId,
        int? countryId,
        DateOnly? issueDate,
        DateOnly? expiryDate,
        CancellationToken cancellationToken)
    {
        await EnsurePersonExistsAsync(personId, cancellationToken);

        if (!await _referenceDataRepository.IdentifierTypeExistsAsync(identifierTypeId, cancellationToken))
        {
            throw new BusinessRuleException($"Identifier type {identifierTypeId} was not found.");
        }

        if (countryId.HasValue && !await _referenceDataRepository.CountryExistsAsync(countryId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Issuing country {countryId.Value} was not found.");
        }

        if (issueDate.HasValue && expiryDate.HasValue && expiryDate.Value < issueDate.Value)
        {
            throw new BusinessRuleException("Identifier expiry date cannot be earlier than issue date.");
        }
    }

    private async Task EnsurePersonExistsAsync(int personId, CancellationToken cancellationToken)
    {
        if (!await _personRepository.ExistsAsync(personId, cancellationToken: cancellationToken))
        {
            throw new EntityNotFoundException($"Person {personId} was not found.");
        }
    }

    private static EntityNotFoundException IdentifierNotFound(int identifierId)
    {
        return new EntityNotFoundException($"Person identifier {identifierId} was not found.");
    }
}
