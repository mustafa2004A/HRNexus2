using System.Globalization;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Notifications;
using HRNexus.Business.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRNexus.Business.Services;

public sealed class LoggingEmployeeTerminationNotificationService : IEmployeeTerminationNotificationService
{
    private readonly ILogger<LoggingEmployeeTerminationNotificationService> _logger;
    private readonly HrNotificationOptions _options;

    public LoggingEmployeeTerminationNotificationService(
        IOptions<HrNotificationOptions> options,
        ILogger<LoggingEmployeeTerminationNotificationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task NotifyEmployeeTerminatedAsync(
        EmployeeTerminationNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // TODO: Resolve responsible recipients from active HR users or Employees.Terminate permission holders when recipient lookup is introduced.
        if (!_options.EnableTerminationEmail && !_options.EnableTerminationSms)
        {
            _logger.LogInformation(
                "Employee termination notifications are disabled. EmployeeCode: {EmployeeCode}",
                notification.EmployeeCode);
            return Task.CompletedTask;
        }

        if (_options.EnableTerminationEmail)
        {
            LogEmailNotification(notification);
        }

        if (_options.EnableTerminationSms)
        {
            LogSmsNotification(notification);
        }

        // TODO: Move employee termination notifications to an outbox/background worker for guaranteed delivery.
        return Task.CompletedTask;
    }

    private void LogEmailNotification(EmployeeTerminationNotification notification)
    {
        if (string.IsNullOrWhiteSpace(_options.TerminationResponsibleEmail))
        {
            _logger.LogWarning(
                "Termination email notification skipped because TerminationResponsibleEmail is not configured. EmployeeCode: {EmployeeCode}",
                notification.EmployeeCode);
            return;
        }

        var subject = $"Employee Termination Notification - {notification.EmployeeCode}";
        var body = BuildEmailBody(notification);

        _logger.LogInformation(
            "Termination email notification would be sent to the configured responsible recipient. EmployeeCode: {EmployeeCode}; Subject: {Subject}; Body: {Body}",
            notification.EmployeeCode,
            subject,
            body);
    }

    private void LogSmsNotification(EmployeeTerminationNotification notification)
    {
        if (string.IsNullOrWhiteSpace(_options.TerminationResponsiblePhoneNumber))
        {
            _logger.LogWarning(
                "Termination SMS notification skipped because TerminationResponsiblePhoneNumber is not configured. EmployeeCode: {EmployeeCode}",
                notification.EmployeeCode);
            return;
        }

        var message = BuildSmsMessage(notification);

        _logger.LogInformation(
            "Termination SMS notification would be sent to the configured responsible recipient. EmployeeCode: {EmployeeCode}; Message: {Message}",
            notification.EmployeeCode,
            message);
    }

    private static string BuildEmailBody(EmployeeTerminationNotification notification)
    {
        return string.Join(
            Environment.NewLine,
            $"Employee Code: {notification.EmployeeCode}",
            $"Employee Full Name: {notification.EmployeeFullName}",
            $"Termination Date: {FormatDate(notification.TerminationDate)}",
            $"Termination Reason: {notification.TerminationReasonName}",
            $"Eligible for Rehire: {(notification.IsEligibleForRehire ? "Yes" : "No")}",
            $"Terminated By: {ResolveActor(notification)}",
            $"Occurred At: {notification.OccurredAt:O}");
    }

    private static string BuildSmsMessage(EmployeeTerminationNotification notification)
    {
        return
            $"Employee {notification.EmployeeCode} was terminated on {FormatDate(notification.TerminationDate)}. " +
            $"Reason: {Truncate(notification.TerminationReasonName, 80)}. " +
            $"By: {Truncate(ResolveActor(notification), 40)}.";
    }

    private static string ResolveActor(EmployeeTerminationNotification notification)
    {
        if (!string.IsNullOrWhiteSpace(notification.TerminatedByUsername))
        {
            return notification.TerminatedByUsername.Trim();
        }

        return notification.TerminatedByUserId.HasValue
            ? $"User {notification.TerminatedByUserId.Value}"
            : "Unknown user";
    }

    private static string FormatDate(DateOnly value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
