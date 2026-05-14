using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Auth;
using HRNexus.Business.Options;
using HRNexus.Business.Security;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.Extensions.Options;

namespace HRNexus.Business.Services;

public sealed class AuthService : IAuthService
{
    private const string ActiveAccountStatusCode = "A";
    private const string LockedAccountStatusCode = "L";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordVerificationService _passwordVerificationService;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly IPermissionService _permissionService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;
    private readonly AuthSecurityOptions _options;

    public AuthService(
        IUserRepository userRepository,
        IPasswordVerificationService passwordVerificationService,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        IUserActivityLogService userActivityLogService,
        IPermissionService permissionService,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext,
        IOptions<AuthSecurityOptions> options)
    {
        _userRepository = userRepository;
        _passwordVerificationService = passwordVerificationService;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _userActivityLogService = userActivityLogService;
        _permissionService = permissionService;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<AuthTokenResponseDto> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var username = NormalizeUsername(request.Username);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BusinessRuleException("Username and password are required.");
        }

        var user = await _userRepository.GetByUsernameForUpdateAsync(username, cancellationToken);

        if (user is null)
        {
            await _userActivityLogService.LogAsync(
                null,
                SecurityActivityCodes.LoginFailed,
                false,
                $"Unknown username: {username}",
                ipAddress,
                cancellationToken);

            throw InvalidCredentials();
        }

        if (!IsActiveUser(user))
        {
            await _userActivityLogService.LogAsync(
                user.UserId,
                SecurityActivityCodes.LoginFailed,
                false,
                "Inactive or locked account login attempt.",
                ipAddress,
                cancellationToken);

            throw InvalidCredentials();
        }

        var passwordResult = _passwordVerificationService.Verify(request.Password, user.PasswordHash);

        if (!passwordResult.Succeeded)
        {
            await HandleFailedLoginAsync(user, passwordResult.FailureReason, ipAddress, cancellationToken);
            throw InvalidCredentials();
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        user.ModifiedDate = GetSafeModifiedDate(user.CreatedDate);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await CreateTokenResponseAsync(user.UserId, user.EmployeeId, user.Username, ipAddress, cancellationToken);

        await _userActivityLogService.LogAsync(
            user.UserId,
            SecurityActivityCodes.LoginSuccess,
            true,
            "Login succeeded.",
            ipAddress,
            cancellationToken);

        return result;
    }

    public async Task<AuthTokenResponseDto> RefreshAsync(RefreshTokenRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var (existingToken, newRefreshToken) = await _refreshTokenService.RotateAsync(request.RefreshToken, ipAddress, cancellationToken);
        var user = await _userRepository.GetIdentityByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new AuthenticationFailedException("User account was not found.");

        var tokenUser = new AccessTokenUser(user.UserId, user.EmployeeId, user.Username, user.Roles);
        var accessToken = _accessTokenService.CreateAccessToken(tokenUser);

        await _userActivityLogService.LogAsync(
            user.UserId,
            SecurityActivityCodes.TokenRefresh,
            true,
            "Refresh token rotated.",
            ipAddress,
            cancellationToken);

        return new AuthTokenResponseDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt,
            new AuthenticatedUserDto(user.UserId, user.EmployeeId, user.Username, user.Roles));
    }

    public async Task LogoutAsync(LogoutRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var userId = await _refreshTokenService.RevokeAsync(request.RefreshToken ?? string.Empty, ipAddress, cancellationToken)
            ?? _currentUserContext.UserId;

        await _userActivityLogService.LogAsync(
            userId,
            SecurityActivityCodes.Logout,
            true,
            "Logout requested.",
            ipAddress,
            cancellationToken);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserContext.UserId
            ?? throw new AuthenticationFailedException("Authenticated user context was not found.");

        var user = await _userRepository.GetIdentityByIdAsync(currentUserId, cancellationToken)
            ?? throw new EntityNotFoundException($"User {currentUserId} was not found.");

        var permissions = await _permissionService.GetPermissionSummariesAsync(currentUserId, cancellationToken);

        return new CurrentUserDto(
            user.UserId,
            user.EmployeeId,
            user.Username,
            user.Roles,
            permissions);
    }

    private async Task<AuthTokenResponseDto> CreateTokenResponseAsync(
        int userId,
        int? employeeId,
        string username,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles = await _permissionService.GetRolesAsync(userId, cancellationToken);
        var tokenUser = new AccessTokenUser(userId, employeeId, username, roles);
        var accessToken = _accessTokenService.CreateAccessToken(tokenUser);
        var refreshToken = await _refreshTokenService.CreateAsync(userId, ipAddress, cancellationToken);

        return new AuthTokenResponseDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            new AuthenticatedUserDto(userId, employeeId, username, roles));
    }

    private async Task HandleFailedLoginAsync(
        User user,
        string? failureReason,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        user.FailedLoginAttempts += 1;
        user.ModifiedDate = GetSafeModifiedDate(user.CreatedDate);

        if (user.FailedLoginAttempts >= Math.Max(1, _options.MaxFailedLoginAttempts))
        {
            var lockedStatus = await _userRepository.GetAccountStatusByCodeAsync(LockedAccountStatusCode, cancellationToken);

            if (lockedStatus is not null)
            {
                user.AccountStatusId = lockedStatus.AccountStatusId;
                user.IsActive = false;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _userActivityLogService.LogAsync(
            user.UserId,
            SecurityActivityCodes.LoginFailed,
            false,
            failureReason is null ? "Login failed." : $"Login failed: {failureReason}",
            ipAddress,
            cancellationToken);

        if (!IsActiveUser(user))
        {
            await _userActivityLogService.LogAsync(
                user.UserId,
                SecurityActivityCodes.AccountLocked,
                false,
                "Account reached failed login threshold.",
                ipAddress,
                cancellationToken);
        }
    }

    private static bool IsActiveUser(User user)
    {
        return user.IsActive && user.AccountStatus.StatusCode == ActiveAccountStatusCode;
    }

    private static string NormalizeUsername(string? username)
    {
        return BusinessValidation.NormalizeOptionalText(username) ?? string.Empty;
    }

    private static AuthenticationFailedException InvalidCredentials()
    {
        return new AuthenticationFailedException("Invalid username or password.");
    }

    private static DateTime GetSafeModifiedDate(DateTime createdDate)
    {
        var now = DateTime.UtcNow;
        return createdDate > now ? createdDate : now;
    }
}
