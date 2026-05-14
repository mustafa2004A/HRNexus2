using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Auth;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class DevelopmentPasswordBootstrapService : IDevelopmentPasswordBootstrapService
{
    private const string ActiveAccountStatusCode = "A";

    private static readonly string[] DefaultDemoUsernames =
    [
        "admin",
        "sarah.haddad",
        "omar.khalil",
        "lina.nasser",
        "nadia.saleh"
    ];

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IHRNexusDbContext _dbContext;

    public DevelopmentPasswordBootstrapService(
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        IHRNexusDbContext dbContext)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _dbContext = dbContext;
    }

    public async Task<DevelopmentPasswordBootstrapResultDto> ReseedDemoPasswordsAsync(
        DevelopmentPasswordBootstrapRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePassword(request.Password);

        var requestedUsernames = NormalizeUsernames(request.Usernames is { Count: > 0 }
            ? request.Usernames
            : DefaultDemoUsernames);

        if (requestedUsernames.Count == 0)
        {
            throw new BusinessRuleException("At least one valid username is required.");
        }

        var users = await _userRepository.GetByUsernamesForUpdateAsync(requestedUsernames, cancellationToken);
        var activeStatus = await _userRepository.GetAccountStatusByCodeAsync(ActiveAccountStatusCode, cancellationToken)
            ?? throw new BusinessRuleException("Active account status was not found.");

        var now = DateTime.UtcNow;

        foreach (var user in users)
        {
            user.PasswordHash = _passwordHashingService.HashPassword(request.Password);
            user.FailedLoginAttempts = 0;
            user.IsActive = true;
            user.AccountStatusId = activeStatus.AccountStatusId;
            user.ModifiedDate = GetSafeModifiedDate(user.CreatedDate, now);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedUsernames = users
            .Select(user => user.Username)
            .OrderBy(username => username)
            .ToArray();

        var updatedLookup = updatedUsernames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingUsernames = requestedUsernames
            .Where(username => !updatedLookup.Contains(username))
            .OrderBy(username => username)
            .ToArray();

        return new DevelopmentPasswordBootstrapResultDto(
            updatedUsernames.Length,
            updatedUsernames,
            missingUsernames,
            DateTime.UtcNow);
    }

    private static IReadOnlyList<string> NormalizeUsernames(IEnumerable<string> usernames)
    {
        return usernames
            .Select(username => BusinessValidation.NormalizeOptionalText(username))
            .Where(username => !string.IsNullOrWhiteSpace(username))
            .Select(username => username!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new BusinessRuleException("Password is required.");
        }

        if (password.Length < 10)
        {
            throw new BusinessRuleException("Development password must be at least 10 characters long.");
        }

        if (password.Any(char.IsWhiteSpace))
        {
            throw new BusinessRuleException("Development password must not contain whitespace.");
        }

        if (!password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit)
            || !password.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new BusinessRuleException("Development password must include uppercase, lowercase, digit, and symbol characters.");
        }
    }

    private static DateTime GetSafeModifiedDate(DateTime createdDate, DateTime now)
    {
        return createdDate > now ? createdDate : now;
    }
}
