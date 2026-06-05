using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Security;
using Microsoft.Extensions.Logging;

namespace HRNexus.Business.Services;

public sealed class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationCodeAsync(
        VerificationCodeDeliveryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Verification code delivery simulated. ActionType: {ActionType}; DeliveryMethod: {DeliveryMethod}; DestinationMasked: {DestinationMasked}; ExpiresAt: {ExpiresAt:o}; CodeLength: {CodeLength}",
            request.ActionType,
            request.DeliveryMethod,
            request.DestinationMasked,
            request.ExpiresAt,
            request.Code.Length);

        return Task.CompletedTask;
    }
}
