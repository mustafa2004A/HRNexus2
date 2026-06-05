using System.Security.Cryptography;
using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Security;
using HRNexus.Business.Options;
using HRNexus.Business.Security;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Security;
using HRNexus.DataAccess.Repositories.Abstractions;
using Microsoft.Extensions.Options;

namespace HRNexus.Business.Services;

public sealed class VerificationCodeService : IVerificationCodeService
{
    private const int DefaultCodeLength = 6;
    private const int MinimumCodeLength = 4;
    private const int MaximumCodeLength = 8;
    private const int DefaultExpiryMinutes = 5;
    private const int DefaultMaxAttempts = 5;
    private const int HashIterations = 100_000;
    private const int SaltLengthBytes = 16;
    private const int HashLengthBytes = 32;
    private const string HashPrefix = "pbkdf2-sha256";
    private const string DeliveryMethodEmail = "Email";
    private const string DeliveryMethodSms = "SMS";

    private readonly IActionVerificationCodeRepository _verificationCodeRepository;
    private readonly INotificationSender _notificationSender;
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly IHRNexusDbContext _dbContext;
    private readonly TerminationVerificationOptions _options;
    private readonly HrNotificationOptions _notificationOptions;

    public VerificationCodeService(
        IActionVerificationCodeRepository verificationCodeRepository,
        INotificationSender notificationSender,
        IUserActivityLogService userActivityLogService,
        IHRNexusDbContext dbContext,
        IOptions<TerminationVerificationOptions> options,
        IOptions<HrNotificationOptions> notificationOptions)
    {
        _verificationCodeRepository = verificationCodeRepository;
        _notificationSender = notificationSender;
        _userActivityLogService = userActivityLogService;
        _dbContext = dbContext;
        _options = options.Value;
        _notificationOptions = notificationOptions.Value;
    }

    public async Task<ActionVerificationCodeIssueResult> IssueCodeAsync(
        ActionVerificationCodeIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_options.Enabled)
        {
            throw new BusinessRuleException("Termination verification is disabled.");
        }

        var deliveryTarget = ResolveDeliveryTarget();
        var code = GenerateNumericCode(GetConfiguredCodeLength());
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(GetConfiguredExpiryMinutes());
        var verificationCode = new ActionVerificationCode
        {
            ActionVerificationCodeId = Guid.NewGuid(),
            ActionType = BusinessValidation.NormalizeRequiredText(request.ActionType, "Action type"),
            TargetEntityType = BusinessValidation.NormalizeRequiredText(request.TargetEntityType, "Target entity type"),
            TargetEntityId = request.TargetEntityId,
            RequestedByUserId = request.RequestedByUserId,
            DeliveryMethod = deliveryTarget.DeliveryMethod,
            DestinationMasked = deliveryTarget.DestinationMasked,
            CodeHash = HashCode(code),
            ExpiresAt = expiresAt,
            AttemptCount = 0,
            MaxAttempts = GetConfiguredMaxAttempts(),
            CreatedAt = now,
            CreatedByIp = Truncate(BusinessValidation.NormalizeOptionalText(request.CreatedByIp), 50),
            IsRevoked = false
        };

        await _verificationCodeRepository.AddAsync(verificationCode, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _notificationSender.SendVerificationCodeAsync(
                new VerificationCodeDeliveryRequest(
                    verificationCode.ActionType,
                    deliveryTarget.DeliveryMethod,
                    deliveryTarget.Destination,
                    deliveryTarget.DestinationMasked,
                    code,
                    expiresAt),
                cancellationToken);
        }
        catch
        {
            verificationCode.IsRevoked = true;
            verificationCode.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await LogAsync(
                request.RequestedByUserId,
                SecurityActivityCodes.TerminationVerificationCodeSendFailed,
                false,
                $"Termination verification code send failed for employee {request.TargetEntityId}.",
                request.CreatedByIp,
                cancellationToken);

            throw new BusinessRuleException("Verification code could not be sent. Please try again.");
        }

        await LogAsync(
            request.RequestedByUserId,
            SecurityActivityCodes.TerminationVerificationCodeRequested,
            true,
            $"Termination verification code requested for employee {request.TargetEntityId}. Delivery: {deliveryTarget.DeliveryMethod}.",
            request.CreatedByIp,
            cancellationToken);

        return new ActionVerificationCodeIssueResult(
            verificationCode.ActionVerificationCodeId,
            deliveryTarget.DeliveryMethod,
            deliveryTarget.DestinationMasked,
            expiresAt,
            "Verification code was sent.");
    }

    public async Task VerifyCodeAsync(
        ActionVerificationCodeVerifyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var verificationCode = await _verificationCodeRepository.GetByIdForUpdateAsync(
            request.VerificationRequestId,
            cancellationToken)
            ?? throw new BusinessRuleException("Verification request was not found.");

        if (!MatchesRequest(verificationCode, request))
        {
            await LogVerificationFailedAsync(request, "Verification request does not match the employee action.", cancellationToken);
            throw new BusinessRuleException("Verification request is invalid for this employee.");
        }

        if (verificationCode.IsRevoked)
        {
            await LogVerificationFailedAsync(request, "Verification request was revoked.", cancellationToken);
            throw new BusinessRuleException("Verification request is no longer valid. Request a new code.");
        }

        if (verificationCode.UsedAt.HasValue)
        {
            await LogVerificationFailedAsync(request, "Verification request was already used.", cancellationToken);
            throw new BusinessRuleException("Verification code has already been used.");
        }

        if (verificationCode.ExpiresAt <= DateTime.UtcNow)
        {
            await LogVerificationFailedAsync(request, "Verification request expired.", cancellationToken);
            throw new BusinessRuleException("Verification code has expired. Request a new code.");
        }

        if (verificationCode.AttemptCount >= verificationCode.MaxAttempts)
        {
            await LogVerificationFailedAsync(request, "Maximum verification attempts exceeded.", cancellationToken);
            throw new BusinessRuleException("Maximum verification attempts exceeded. Request a new code.");
        }

        var normalizedCode = BusinessValidation.NormalizeRequiredText(request.VerificationCode, "Verification code");
        if (!VerifyHashedCode(normalizedCode, verificationCode.CodeHash))
        {
            verificationCode.AttemptCount += 1;

            if (verificationCode.AttemptCount >= verificationCode.MaxAttempts)
            {
                verificationCode.IsRevoked = true;
                verificationCode.RevokedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await LogVerificationFailedAsync(request, "Maximum verification attempts exceeded.", cancellationToken);
                throw new BusinessRuleException("Maximum verification attempts exceeded. Request a new code.");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await LogVerificationFailedAsync(request, "Incorrect verification code.", cancellationToken);
            throw new BusinessRuleException("Verification code is incorrect.");
        }

        verificationCode.AttemptCount += 1;
        verificationCode.UsedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await LogAsync(
            request.RequestedByUserId,
            SecurityActivityCodes.TerminationVerificationSucceeded,
            true,
            $"Termination verification succeeded for employee {request.TargetEntityId}.",
            request.ClientIpAddress,
            cancellationToken);
    }

    private DeliveryTarget ResolveDeliveryTarget()
    {
        var preferredMethod = NormalizeDeliveryMethod(_options.PreferredDeliveryMethod);
        var email = FirstConfigured(_options.FallbackEmail, _notificationOptions.TerminationResponsibleEmail);
        var phoneNumber = FirstConfigured(_options.FallbackPhoneNumber, _notificationOptions.TerminationResponsiblePhoneNumber);

        if (preferredMethod == DeliveryMethodSms && phoneNumber is not null)
        {
            return new DeliveryTarget(DeliveryMethodSms, phoneNumber, MaskPhoneNumber(phoneNumber));
        }

        if (preferredMethod == DeliveryMethodEmail && email is not null)
        {
            return new DeliveryTarget(DeliveryMethodEmail, email, MaskEmail(email));
        }

        if (email is not null)
        {
            return new DeliveryTarget(DeliveryMethodEmail, email, MaskEmail(email));
        }

        if (phoneNumber is not null)
        {
            return new DeliveryTarget(DeliveryMethodSms, phoneNumber, MaskPhoneNumber(phoneNumber));
        }

        throw new BusinessRuleException("Termination verification destination is not configured.");
    }

    private int GetConfiguredCodeLength()
    {
        return _options.CodeLength is >= MinimumCodeLength and <= MaximumCodeLength
            ? _options.CodeLength
            : DefaultCodeLength;
    }

    private int GetConfiguredExpiryMinutes()
    {
        return _options.ExpiryMinutes > 0 ? _options.ExpiryMinutes : DefaultExpiryMinutes;
    }

    private int GetConfiguredMaxAttempts()
    {
        return _options.MaxAttempts > 0 ? _options.MaxAttempts : DefaultMaxAttempts;
    }

    private static bool MatchesRequest(
        ActionVerificationCode verificationCode,
        ActionVerificationCodeVerifyRequest request)
    {
        return verificationCode.RequestedByUserId == request.RequestedByUserId
            && verificationCode.TargetEntityId == request.TargetEntityId
            && string.Equals(verificationCode.ActionType, request.ActionType, StringComparison.Ordinal)
            && string.Equals(verificationCode.TargetEntityType, request.TargetEntityType, StringComparison.Ordinal);
    }

    private static string GenerateNumericCode(int codeLength)
    {
        var maximumValue = (int)Math.Pow(10, codeLength);
        var value = RandomNumberGenerator.GetInt32(0, maximumValue);
        return value.ToString($"D{codeLength}");
    }

    private static string HashCode(string code)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLengthBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            code,
            salt,
            HashIterations,
            HashAlgorithmName.SHA256,
            HashLengthBytes);

        return $"{HashPrefix}${HashIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyHashedCode(string code, string storedHash)
    {
        try
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 4 || !string.Equals(parts[0], HashPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                code,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private async Task LogVerificationFailedAsync(
        ActionVerificationCodeVerifyRequest request,
        string details,
        CancellationToken cancellationToken)
    {
        await LogAsync(
            request.RequestedByUserId,
            SecurityActivityCodes.TerminationVerificationFailed,
            false,
            $"Termination verification failed for employee {request.TargetEntityId}. {details}",
            request.ClientIpAddress,
            cancellationToken);
    }

    private async Task LogAsync(
        int userId,
        string activityTypeCode,
        bool isSuccess,
        string details,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userActivityLogService.LogAsync(userId, activityTypeCode, isSuccess, details, ipAddress, cancellationToken);
        }
        catch
        {
            // Verification activity logging must not leak codes or block the API operation.
        }
    }

    private static string NormalizeDeliveryMethod(string? deliveryMethod)
    {
        return string.Equals(deliveryMethod, DeliveryMethodSms, StringComparison.OrdinalIgnoreCase)
            ? DeliveryMethodSms
            : DeliveryMethodEmail;
    }

    private static string? FirstConfigured(params string?[] values)
    {
        foreach (var value in values)
        {
            var normalized = BusinessValidation.NormalizeOptionalText(value);
            if (normalized is not null)
            {
                return normalized;
            }
        }

        return null;
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return "***";
        }

        var local = email[..atIndex];
        var domain = email[(atIndex + 1)..];
        var visible = local[0];
        return $"{visible}***@{domain}";
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length <= 4)
        {
            return "***";
        }

        return $"***{digits[^4..]}";
    }

    private static string? Truncate(string? value, int maxLength)
    {
        return value is null || value.Length <= maxLength ? value : value[..maxLength];
    }

    private sealed record DeliveryTarget(string DeliveryMethod, string Destination, string DestinationMasked);
}
