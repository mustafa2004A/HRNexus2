using System.Security.Cryptography;
using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Auth;
using HRNexus.Business.Options;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.Extensions.Options;

namespace HRNexus.Business.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IHRNexusDbContext _dbContext;
    private readonly AuthSecurityOptions _options;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IHRNexusDbContext dbContext,
        IOptions<AuthSecurityOptions> options)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<RefreshTokenResult> CreateAsync(int userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var token = CreatePlainToken();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(Math.Max(1, _options.RefreshTokenExpirationDays));

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(token),
            CreatedAt = now,
            ExpiresAt = expiresAt,
            CreatedByIp = NormalizeIp(ipAddress)
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(token, expiresAt);
    }

    public async Task<(RefreshToken ExistingToken, RefreshTokenResult NewToken)> RotateAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await GetValidTokenAsync(refreshToken, asTracking: true, cancellationToken);
        var newToken = await CreateAsync(existingToken.UserId, ipAddress, cancellationToken);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = NormalizeIp(ipAddress);

        var replacement = await _refreshTokenRepository.GetByTokenHashAsync(HashToken(newToken.Token), cancellationToken: cancellationToken)
            ?? throw new AuthenticationFailedException("Unable to rotate refresh token.");

        existingToken.ReplacedByTokenId = replacement.RefreshTokenId;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (existingToken, newToken);
    }

    public async Task<int?> RevokeAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var tokenHash = HashToken(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, asTracking: true, cancellationToken);

        if (existingToken is null)
        {
            return null;
        }

        if (existingToken.RevokedAt is null)
        {
            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.RevokedByIp = NormalizeIp(ipAddress);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return existingToken.UserId;
    }

    private async Task<RefreshToken> GetValidTokenAsync(
        string refreshToken,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AuthenticationFailedException("Refresh token is required.");
        }

        var tokenHash = HashToken(refreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, asTracking, cancellationToken)
            ?? throw new AuthenticationFailedException("Refresh token is invalid.");

        if (token.RevokedAt.HasValue)
        {
            throw new AuthenticationFailedException("Refresh token has been revoked.");
        }

        if (token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new AuthenticationFailedException("Refresh token has expired.");
        }

        if (!token.User.IsActive || token.User.AccountStatus.StatusCode != "A")
        {
            throw new AuthenticationFailedException("User account is not active.");
        }

        return token;
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private static string CreatePlainToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string? NormalizeIp(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        var trimmed = BusinessValidation.NormalizeOptionalText(ipAddress);
        if (trimmed is null)
        {
            return null;
        }

        return trimmed.Length <= 45 ? trimmed : trimmed[..45];
    }
}
